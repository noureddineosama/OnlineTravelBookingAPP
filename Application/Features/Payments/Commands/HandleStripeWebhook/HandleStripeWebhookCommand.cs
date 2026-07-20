using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Commands.HandleStripeWebhook;

public sealed record HandleStripeWebhookCommand(
    string Json,
    string StripeSignature
) : IRequest<ApiResponse<string>>;

public sealed class HandleStripeWebhookCommandHandler
    : IRequestHandler<HandleStripeWebhookCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGatewayService _paymentGateway;

    public HandleStripeWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentGatewayService paymentGateway)
    {
        _context = context;
        _paymentGateway = paymentGateway;
    }

    public async Task<ApiResponse<string>> Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var webhook = _paymentGateway.ParseWebhookEvent(
            request.Json,
            request.StripeSignature);

        if (!webhook.IsPaymentEvent)
        {
            return ApiResponse<string>.Ok(
                webhook.EventType,
                "Webhook received, but it is not a payment event.");
        }

        var payment = await _context.payments
            .Include(p => p.booking)
            .FirstOrDefaultAsync(
                p => p.transaction_id == webhook.PaymentIntentId,
                cancellationToken);

        if (payment is null)
        {
            return ApiResponse<string>.Fail(
                $"Payment with transaction id '{webhook.PaymentIntentId}' was not found.", 404);
        }

        if (webhook.Status == "succeeded")
        {
            payment.status = "succeeded";
            payment.booking.payment_status = "paid";
            payment.booking.status = BookingStatus.Confirmed.ToString();
            payment.booking.updated_at = DateTime.UtcNow;
        }
        else if (webhook.Status == "failed")
        {
            payment.status = "failed";
            payment.booking.payment_status = "failed";
            payment.booking.updated_at = DateTime.UtcNow;
        }
        else if (webhook.Status == "canceled")
        {
            payment.status = "canceled";
            payment.booking.payment_status = "unpaid";
            payment.booking.updated_at = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok(
            webhook.EventType,
            "Stripe webhook processed successfully.");
    }
}