using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Payments.Providers;

// SIMULATED: no real API calls. In a real app this would use a typed HttpClient.
public class MercadoPagoProvider(ILogger<MercadoPagoProvider> logger) : IRefundablePaymentProvider
{
    private const decimal MaxAmount = 5_000_000m;

    public string Name => "mercadopago";
    public string DisplayName => "Mercado Pago";

    public async Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct)
    {
        await Task.Delay(200, ct); // simulates network latency

        if (order.Total > MaxAmount)
            return PaymentResult.Rejected($"Mercado Pago rejected the payment: amount exceeds {MaxAmount:N0}.");

        var paymentId = ($"MP-{Guid.NewGuid():N}")[..15];
        logger.LogInformation("[MercadoPago] Charged {Total} -> {PaymentId}", order.Total, paymentId);

        return PaymentResult.Approved(paymentId);
    }

    public async Task RefundAsync(string paymentId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(200, ct);
        logger.LogInformation("[MercadoPago] Refunded {Amount} for {PaymentId}", amount, paymentId);
    }
}
