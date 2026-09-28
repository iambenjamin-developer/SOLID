namespace SolidShop.Api.Features.Products;

// Requests
public record CreateProductRequest(string Name, string? Description, string Sku, decimal Price, int Stock);

public record UpdateProductRequest(string Name, string? Description, decimal Price);

public record SetStockRequest(int Stock);

public record ProductFilter(string? Search, int Page = 1, int PageSize = 10);

// Responses (we never return the entity directly)
public record ProductResponse(
    int Id,
    string Name,
    string Description,
    string Sku,
    string Slug,
    decimal Price,
    int Stock,
    bool InStock);

public record ProductListItemResponse(
    int Id,
    string Name,
    string ShortDescription,
    string Slug,
    decimal Price,
    bool InStock);
