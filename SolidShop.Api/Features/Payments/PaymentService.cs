using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Payments;

public interface IPaymentService
{
    IReadOnlyList<PaymentMethodResponse> GetAvailableMethods();
    Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct);
    Task<bool> TryRefundAsync(Order order, CancellationToken ct);
}

// Open/Closed: this class NEVER changes when a new payment method is added.
// It receives ALL registered IPaymentProvider and picks one by name.
public class PaymentService(
    IEnumerable<IPaymentProvider> providers,
    ILogger<PaymentService> logger) : IPaymentService
{
    public IReadOnlyList<PaymentMethodResponse> GetAvailableMethods() =>
        providers
            .Select(p => new PaymentMethodResponse(p.Name, p.DisplayName, p is IRefundablePaymentProvider))
            .ToList();

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct) =>
        Resolve(order.PaymentMethod).ChargeAsync(order, ct);

    public async Task<bool> TryRefundAsync(Order order, CancellationToken ct)
    {
        var provider = Resolve(order.PaymentMethod);

        if (provider is not IRefundablePaymentProvider refundable)
        {
            logger.LogWarning("Order {OrderId}: '{Method}' doesn't support automatic refunds", order.Id, provider.Name);
            return false;
        }

        await refundable.RefundAsync(order.PaymentId!, order.Total, ct);
        return true;
    }

    private IPaymentProvider Resolve(string method) =>
        providers.FirstOrDefault(p => p.Name.Equals(method, StringComparison.OrdinalIgnoreCase))
        ?? throw new BusinessRuleException(
            $"Payment method '{method}' is not supported. Available: {string.Join(", ", providers.Select(p => p.Name))}.");
}
