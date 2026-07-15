using Application.Common.Interfaces;
using Application.Common.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Services;

public sealed class StripeService : IStripeService
{
    public StripeService(IOptions<StripeSettings> options)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public async Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingNumber,
        CancellationToken cancellationToken = default)
    {
        var amountInSmallestUnit = ToStripeAmount(amount, currency);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = currency.ToLowerInvariant(),
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                ["booking_number"] = bookingNumber
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(
            options,
            requestOptions: null,
            cancellationToken: cancellationToken);

        return new StripePaymentIntentResult(
            intent.Id,
            intent.ClientSecret);
    }

    public async Task<string> RefundPaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };

        if (amount.HasValue)
        {
            options.Amount = ToStripeAmount(amount.Value, "USD");
        }

        var service = new RefundService();
        var refund = await service.CreateAsync(
            options,
            requestOptions: null,
            cancellationToken: cancellationToken);

        return refund.Id;
    }

    private static long ToStripeAmount(decimal amount, string currency)
    {
        var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF",
            "KRW", "MGA", "PYG", "RWF", "UGX",
            "VND", "VUV", "XAF", "XOF", "XPF"
        };

        if (zeroDecimalCurrencies.Contains(currency))
            return decimal.ToInt64(decimal.Round(amount, 0, MidpointRounding.AwayFromZero));

        return decimal.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero));
    }
}