using SolidShop.Api.Common.Extensions;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Products;

// Dependency Inversion: ProductAdminService only knows this interface.
// Today it logs; tomorrow it can send an email or a Slack message
// by registering another implementation, without touching the service.
public interface IProductNotifier
{
    Task ProductCreatedAsync(Product product, CancellationToken ct);
}

public class LoggingProductNotifier(ILogger<LoggingProductNotifier> logger) : IProductNotifier
{
    public Task ProductCreatedAsync(Product product, CancellationToken ct)
    {
        logger.LogInformation("[Notification to stock team] New product {Sku}: {Name}",
            product.Sku, product.Name.Truncate(40));

        return Task.CompletedTask;
    }
}
