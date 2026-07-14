using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Payments;

public sealed class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly StripeOptions _options;

    public StripePaymentGatewayService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;

        var amountInSmallestUnit = ConvertToSmallestCurrencyUnit(
            request.Amount,
            request.Currency);

        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = request.Currency.ToLowerInvariant(),
            Description = request.Description,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                ["booking_id"] = request.BookingId.ToString()
            }
        };

        var service = new PaymentIntentService();

        var paymentIntent = await service.CreateAsync(
            paymentIntentOptions,
            cancellationToken: cancellationToken);

        return new CreatePaymentIntentResult
        {
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status
        };
    }

    public StripeWebhookResult ParseWebhookEvent(
        string json,
        string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            stripeSignature,
            _options.WebhookSecret);

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return new StripeWebhookResult
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                IsPaymentEvent = false
            };
        }

        return stripeEvent.Type switch
        {
            "payment_intent.succeeded" => new StripeWebhookResult
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PaymentIntentId = paymentIntent.Id,
                Status = "succeeded",
                IsPaymentEvent = true
            },

            "payment_intent.payment_failed" => new StripeWebhookResult
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PaymentIntentId = paymentIntent.Id,
                Status = "failed",
                IsPaymentEvent = true
            },

            "payment_intent.canceled" => new StripeWebhookResult
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PaymentIntentId = paymentIntent.Id,
                Status = "canceled",
                IsPaymentEvent = true
            },

            _ => new StripeWebhookResult
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                IsPaymentEvent = false
            }
        };
    }

    private static long ConvertToSmallestCurrencyUnit(
        decimal amount,
        string currency)
    {
        var normalizedCurrency = currency.ToUpperInvariant();

        var zeroDecimalCurrencies = new HashSet<string>
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF",
            "KRW", "MGA", "PYG", "RWF", "UGX", "VND",
            "VUV", "XAF", "XOF", "XPF"
        };

        if (zeroDecimalCurrencies.Contains(normalizedCurrency))
        {
            return (long)Math.Round(amount, MidpointRounding.AwayFromZero);
        }

        return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }
}