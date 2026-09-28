using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Payments.Providers;

// SIMULATED: no real API calls.
public class StripeProvider(ILogger<StripeProvider> logger) : IRefundablePaymentProvider
{
    public string Name => "stripe";
    public string DisplayName => "Stripe (credit card)";

    public async Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct)
    {
        await Task.Delay(200, ct);

        var paymentId = ($"pi_{Guid.NewGuid():N}")[..18];
        logger.LogInformation("[Stripe] Charged {Total} -> {PaymentId}", order.Total, paymentId);

        return PaymentResult.Approved(paymentId);
    }

    public async Task RefundAsync(string paymentId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(200, ct);
        logger.LogInformation("[Stripe] Refunded {Amount} for {PaymentId}", amount, paymentId);
    }
}
