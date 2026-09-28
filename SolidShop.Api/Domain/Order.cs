using SolidShop.Api.Common.Exceptions;

namespace SolidShop.Api.Domain;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled,
    CancelledPendingManualRefund
}

public class Order
{
    private readonly List<OrderItem> _items = [];

    public int Id { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? PaymentId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { } // required by EF Core

    public static Order Create(string paymentMethod, DateTimeOffset now) =>
        new()
        {
            PaymentMethod = paymentMethod.Trim().ToLowerInvariant(),
            Status = OrderStatus.Pending,
            CreatedAt = now
        };

    public void AddItem(Product product, int quantity)
    {
        product.DecreaseStock(quantity); // the product validates its own stock

        _items.Add(OrderItem.Create(product, quantity));
        Total = _items.Sum(i => i.Subtotal);
    }

    public void Confirm(string paymentId)
    {
        if (Status != OrderStatus.Pending)
            throw new ConflictException($"Order can't be confirmed from status '{Status}'.");

        PaymentId = paymentId;
        Status = OrderStatus.Confirmed;
    }

    public void EnsureCanBeCancelled()
    {
        if (Status != OrderStatus.Confirmed)
            throw new ConflictException($"Order {Id} can't be cancelled because its status is '{Status}'.");
    }

    public void Cancel(bool refunded, DateTimeOffset now)
    {
        EnsureCanBeCancelled();

        Status = refunded ? OrderStatus.Cancelled : OrderStatus.CancelledPendingManualRefund;
        CancelledAt = now;
    }
}

public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty; // snapshot: the name at purchase time
    public decimal UnitPrice { get; private set; }                  // snapshot: the price at purchase time
    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice * Quantity;

    private OrderItem() { } // required by EF Core

    internal static OrderItem Create(Product product, int quantity) =>
        new()
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = quantity
        };
}
