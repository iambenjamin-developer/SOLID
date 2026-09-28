using FluentValidation;
using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Data.Repositories;

namespace SolidShop.Api.Features.Products;

// Interface Segregation: used by the admin panel (manual adjustment) and by the ERP background job
public interface IStockService
{
    Task<ProductResponse> SetStockAsync(int productId, SetStockRequest request, CancellationToken ct);
    Task<int> SyncFromErpAsync(CancellationToken ct);
}

public class StockService(
    IProductRepository repository,
    IValidator<SetStockRequest> validator,
    TimeProvider clock,
    ILogger<StockService> logger) : IStockService
{
    // Simulated ERP value. In a real app this would come from an HTTP client to the ERP.
    private const int ErpRestockQuantity = 10;

    public async Task<ProductResponse> SetStockAsync(int productId, SetStockRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var product = await repository.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException($"Product {productId} not found.");

        product.SetStock(request.Stock, clock.GetUtcNow());
        await repository.SaveChangesAsync(ct);

        return product.ToResponse();
    }

    public async Task<int> SyncFromErpAsync(CancellationToken ct)
    {
        var outOfStock = await repository.GetOutOfStockAsync(ct);
        if (outOfStock.Count == 0) return 0;

        var now = clock.GetUtcNow();
        foreach (var product in outOfStock)
        {
            product.SetStock(ErpRestockQuantity, now);
            logger.LogInformation("[ERP] Restocked {Sku} ({Name}) with {Quantity} units",
                product.Sku, product.Name, ErpRestockQuantity);
        }

        await repository.SaveChangesAsync(ct);
        return outOfStock.Count;
    }
}
