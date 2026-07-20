using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Settings;
using Application.Features.Payments.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.Commands.CreatePaymentIntent;

public sealed class CreatePaymentIntentCommandHandler
    : IRequestHandler<CreatePaymentIntentCommand, ApiResponse<PaymentIntentResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStripeService _stripeService;
    private readonly StripeSettings _stripeSettings;

    public CreatePaymentIntentCommandHandler(
        IApplicationDbContext context,
        IStripeService stripeService,
        IOptions<StripeSettings> stripeOptions)
    {
        _context = context;
        _stripeService = stripeService;
        _stripeSettings = stripeOptions.Value;
    }

    public async Task<ApiResponse<PaymentIntentResponse>> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _context.bookings
            .Include(b => b.payments)
            .FirstOrDefaultAsync(
                b => b.id == request.BookingId && b.user_id == request.UserId,
                cancellationToken);

        if (booking is null)
            return ApiResponse<PaymentIntentResponse>.Fail("Booking not found.", 404);

        if (booking.status != BookingStatus.Pending.ToString())
            return ApiResponse<PaymentIntentResponse>.Fail("Only pending bookings can be paid.", 409);

        if (string.Equals(booking.payment_status, "paid", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<PaymentIntentResponse>.Fail("Booking is already paid.", 409);

        if (booking.total_price <= 0)
            return ApiResponse<PaymentIntentResponse>.Fail("Invalid booking amount.", 400);

        var stripeIntent = await _stripeService.CreatePaymentIntentAsync(
            booking.total_price,
            booking.currency,
            booking.booking_number,
            cancellationToken);

        var payment = booking.payments
            .FirstOrDefault(p =>
                p.gateway == "stripe" &&
                p.status != "paid");

        if (payment is null)
        {
            payment = new payment
            {
                booking_id = booking.id,
                amount = booking.total_price,
                currency = booking.currency,
                gateway = "stripe",
                status = "pending",
                transaction_id = stripeIntent.PaymentIntentId,
                created_at = DateTime.UtcNow
            };

            _context.payments.Add(payment);
        }
        else
        {
            payment.amount = booking.total_price;
            payment.currency = booking.currency;
            payment.status = "pending";
            payment.transaction_id = stripeIntent.PaymentIntentId;
        }

        booking.payment_status = "pending";

        await _context.SaveChangesAsync(cancellationToken);

        var response = new PaymentIntentResponse
        {
            ClientSecret = stripeIntent.ClientSecret,
            PublishableKey = _stripeSettings.PublishableKey,
            PaymentIntentId = stripeIntent.PaymentIntentId,
            Amount = booking.total_price,
            Currency = booking.currency,
            BookingNumber = booking.booking_number
        };

        return ApiResponse<PaymentIntentResponse>.Ok(
            response,
            "PaymentIntent created successfully.");
    }
}