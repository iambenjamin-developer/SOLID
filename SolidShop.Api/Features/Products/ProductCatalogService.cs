using SolidShop.Api.Common;
using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Data.Repositories;

namespace SolidShop.Api.Features.Products;

// Interface Segregation: the public storefront only needs to READ products.
// It doesn't know that create, update, delete or stock operations exist.
public interface IProductCatalogService
{
    Task<PagedResult<ProductListItemResponse>> SearchAsync(ProductFilter filter, CancellationToken ct);
    Task<ProductResponse> GetBySlugAsync(string slug, CancellationToken ct);
}

public class ProductCatalogService(IProductRepository repository) : IProductCatalogService
{
    private const int MaxPageSize = 50;

    public async Task<PagedResult<ProductListItemResponse>> SearchAsync(ProductFilter filter, CancellationToken ct)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

        var result = await repository.SearchAsync(filter.Search, page, pageSize, ct);

        return result.Map(p => p.ToListItem());
    }

    public async Task<ProductResponse> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var product = await repository.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException($"Product '{slug}' not found.");

        return product.ToResponse();
    }
}
