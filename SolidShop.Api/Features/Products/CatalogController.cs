using Microsoft.AspNetCore.Mvc;
using SolidShop.Api.Common;

namespace SolidShop.Api.Features.Products;

// Public storefront. Depends only on the read service (Interface Segregation).
[ApiController]
[Route("api/catalog/products")]
public class CatalogController(IProductCatalogService catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> Search(
        [FromQuery] ProductFilter filter, CancellationToken ct) =>
        Ok(await catalog.SearchAsync(filter, ct));

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetBySlug(string slug, CancellationToken ct) =>
        Ok(await catalog.GetBySlugAsync(slug, ct));
}
