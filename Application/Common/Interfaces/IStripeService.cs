namespace Application.Common.Interfaces;

public interface IStripeService
{
    Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingNumber,
        CancellationToken cancellationToken = default);

    Task<string> RefundPaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default);
}

public sealed record StripePaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret);