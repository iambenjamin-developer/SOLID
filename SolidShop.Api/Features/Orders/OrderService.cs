using FluentValidation;
using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Data.Repositories;
using SolidShop.Api.Domain;
using SolidShop.Api.Features.Payments;

namespace SolidShop.Api.Features.Orders;

public interface IOrderService
{
    Task<OrderResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct);
    Task<CancelOrderResponse> CancelAsync(int id, CancellationToken ct);
}

public class OrderService(
    IOrderRepository orders,
    IProductRepository products,
    IPaymentService payments,
    IValidator<CreateOrderRequest> validator,
    TimeProvider clock) : IOrderService
{
    public async Task<OrderResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var order = await GetOrderOrThrowAsync(id, ct);
        return order.ToResponse();
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var found = await products.GetByIdsAsync(productIds, ct);

        var missing = productIds.Except(found.Select(p => p.Id)).ToList();
        if (missing.Count > 0)
            throw new NotFoundException($"Products not found: {string.Join(", ", missing)}.");

        var order = Order.Create(request.PaymentMethod, clock.GetUtcNow());

        foreach (var item in request.Items)
            order.AddItem(found.Single(p => p.Id == item.ProductId), item.Quantity); // decreases stock

        // Nothing is saved yet. If the payment fails, the stock changes are discarded
        // together with the DbContext when the request ends.
        var payment = await payments.ChargeAsync(order, ct);
        if (!payment.Success)
            throw new BusinessRuleException(payment.Message ?? "Payment rejected.");

        order.Confirm(payment.PaymentId!);

        orders.Add(order);
        await orders.SaveChangesAsync(ct); // saves order + stock in one transaction (same scoped DbContext)

        return order.ToResponse();
    }

    public async Task<CancelOrderResponse> CancelAsync(int id, CancellationToken ct)
    {
        var order = await GetOrderOrThrowAsync(id, ct);
        order.EnsureCanBeCancelled();

        // Give the stock back (products deleted in the meantime are skipped)
        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var orderProducts = await products.GetByIdsAsync(productIds, ct);

        foreach (var item in order.Items)
            orderProducts.FirstOrDefault(p => p.Id == item.ProductId)?.IncreaseStock(item.Quantity);

        // Liskov in action: we don't assume every provider can refund
        var refunded = await payments.TryRefundAsync(order, ct);

        order.Cancel(refunded, clock.GetUtcNow());
        await orders.SaveChangesAsync(ct);

        var message = refunded
            ? "Order cancelled and payment refunded."
            : $"Order cancelled. '{order.PaymentMethod}' doesn't support automatic refunds: a manual refund is required.";

        return new CancelOrderResponse(order.ToResponse(), refunded, message);
    }

    private async Task<Order> GetOrderOrThrowAsync(int id, CancellationToken ct) =>
        await orders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Order {id} not found.");
}
