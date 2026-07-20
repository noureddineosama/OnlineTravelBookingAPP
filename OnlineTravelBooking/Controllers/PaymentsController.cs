using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Features.Payments.Commands.CreatePaymentIntent;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IApplicationDbContext _context;
    private readonly StripeSettings _stripeSettings;

    public PaymentsController(
        ISender mediator,
        IApplicationDbContext context,
        IOptions<StripeSettings> stripeOptions)
    {
        _mediator = mediator;
        _context = context;
        _stripeSettings = stripeOptions.Value;
    }

    [HttpPost("create-intent")]
    [Authorize]
    public async Task<IActionResult> CreateIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user token.");

        var result = await _mediator.Send(
            new CreatePaymentIntentCommand(request.BookingId, userId),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync(cancellationToken);

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeSettings.WebhookSecret);
        }
        catch
        {
            return BadRequest("Invalid Stripe signature.");
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                await HandlePaymentSucceeded(stripeEvent, cancellationToken);
                break;

            case EventTypes.PaymentIntentPaymentFailed:
                await HandlePaymentFailed(stripeEvent, cancellationToken);
                break;
        }

        return Ok();
    }

    private async Task HandlePaymentSucceeded(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;

        if (intent is null)
            return;

        var payment = await _context.payments
            .Include(p => p.booking)
            .FirstOrDefaultAsync(
                p => p.transaction_id == intent.Id,
                cancellationToken);

        if (payment is null)
            return;

        if (payment.status == "paid")
            return;

        payment.status = "paid";

        payment.booking.payment_status = "paid";
        payment.booking.status = BookingStatus.Confirmed.ToString ();

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandlePaymentFailed(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;

        if (intent is null)
            return;

        var payment = await _context.payments
            .Include(p => p.booking)
            .FirstOrDefaultAsync(
                p => p.transaction_id == intent.Id,
                cancellationToken);

        if (payment is null)
            return;

        payment.status = "failed";

        payment.booking.payment_status = "failed";
        payment.booking.status = BookingStatus.Cancelled.ToString();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed record CreatePaymentIntentRequest(long BookingId);