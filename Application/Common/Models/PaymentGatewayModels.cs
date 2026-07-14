namespace Application.Common.Models;

public sealed class CreatePaymentIntentRequest
{
    public long BookingId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Description { get; init; } = string.Empty;
}

public sealed class CreatePaymentIntentResult
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class StripeWebhookResult
{
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string PaymentIntentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsPaymentEvent { get; init; }
}