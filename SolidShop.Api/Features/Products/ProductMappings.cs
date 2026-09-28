using SolidShop.Api.Common.Extensions;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Products;

// Extension methods for mapping: the entity doesn't know about DTOs (Single Responsibility)
public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Sku, p.Slug, p.Price, p.Stock, p.Stock > 0);

    public static ProductListItemResponse ToListItem(this Product p) =>
        new(p.Id, p.Name, p.Description.Truncate(60), p.Slug, p.Price, p.Stock > 0);
}
