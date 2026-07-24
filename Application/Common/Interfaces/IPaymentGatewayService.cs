
using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IPaymentGatewayService
{
    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default);

    StripeWebhookResult ParseWebhookEvent(
        string json,
        string stripeSignature);
}