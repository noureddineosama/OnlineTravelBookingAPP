namespace Application.Features.Payments.DTOs;

public sealed class PaymentIntentResponse
{
    public string ClientSecret { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string PaymentIntentId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string BookingNumber { get; init; } = string.Empty;
}