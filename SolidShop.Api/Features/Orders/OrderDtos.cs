using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Orders;

public record CreateOrderRequest(string PaymentMethod, List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record OrderItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);

public record OrderResponse(
    int Id,
    OrderStatus Status,
    string PaymentMethod,
    string? PaymentId,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    List<OrderItemResponse> Items);

public record CancelOrderResponse(OrderResponse Order, bool Refunded, string Message);
