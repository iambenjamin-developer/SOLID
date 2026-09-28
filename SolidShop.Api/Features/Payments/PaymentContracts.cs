using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Payments;

public record PaymentResult(bool Success, string? PaymentId, string? Message = null)
{
    public static PaymentResult Approved(string paymentId) => new(true, paymentId);
    public static PaymentResult Rejected(string message) => new(false, null, message);
}

// Open/Closed: every payment method is a new class that implements this interface.
public interface IPaymentProvider
{
    string Name { get; }         // key used in the request: "mercadopago", "stripe"...
    string DisplayName { get; }
    Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct);
}

// Liskov Substitution: refunding is NOT part of IPaymentProvider,
// because not every payment method can do it (cash on delivery can't).
// If it were, CashOnDeliveryProvider would have to throw NotSupportedException
// and break any code that trusts the interface.
public interface IRefundablePaymentProvider : IPaymentProvider
{
    Task RefundAsync(string paymentId, decimal amount, CancellationToken ct);
}

public record PaymentMethodResponse(string Name, string DisplayName, bool SupportsRefunds);
