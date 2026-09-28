using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Orders;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order o) =>
        new(o.Id, o.Status, o.PaymentMethod, o.PaymentId, o.Total, o.CreatedAt, o.CancelledAt,
            o.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.Subtotal)).ToList());
}
