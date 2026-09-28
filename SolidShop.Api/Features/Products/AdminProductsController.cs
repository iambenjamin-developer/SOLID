using Microsoft.AspNetCore.Mvc;

namespace SolidShop.Api.Features.Products;

// Single Responsibility: the controller only handles HTTP (routes, status codes).
// No validation, no database, no business rules here.
[ApiController]
[Route("api/admin/products")]
public class AdminProductsController(IProductAdminService admin, IStockService stock) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await admin.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = await admin.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request, CancellationToken ct) =>
        Ok(await admin.UpdateAsync(id, request, ct));

    [HttpPatch("{id:int}/stock")]
    public async Task<ActionResult<ProductResponse>> SetStock(int id, SetStockRequest request, CancellationToken ct) =>
        Ok(await stock.SetStockAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await admin.DeleteAsync(id, ct);
        return NoContent();
    }
}
