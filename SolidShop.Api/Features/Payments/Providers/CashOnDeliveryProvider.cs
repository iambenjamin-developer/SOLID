using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Payments.Providers;

// Only implements IPaymentProvider: it can charge, but it CAN'T refund automatically.
// It doesn't have a RefundAsync that throws an exception -> respects Liskov.
public class CashOnDeliveryProvider(ILogger<CashOnDeliveryProvider> logger) : IPaymentProvider
{
    private const decimal MaxAmount = 300_000m;

    public string Name => "cash-on-delivery";
    public string DisplayName => "Cash on delivery";

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct)
    {
        if (order.Total > MaxAmount)
            return Task.FromResult(PaymentResult.Rejected($"Cash on delivery is only available for orders up to {MaxAmount:N0}."));

        var paymentId = ($"COD-{Guid.NewGuid():N}")[..16];
        logger.LogInformation("[CashOnDelivery] Order registered for {Total} -> {PaymentId}", order.Total, paymentId);

        return Task.FromResult(PaymentResult.Approved(paymentId));
    }
}
