using SolidShop.Api.Features.Products;

namespace SolidShop.Api.BackgroundJobs;

// A BackgroundService is a SINGLETON: it lives for the whole app.
// It CAN'T receive IStockService (Scoped) in the constructor -> "captive dependency".
// .NET throws in Development: "Cannot consume scoped service ... from singleton".
// Solution: inject IServiceScopeFactory and create a scope on every run.
public class ErpStockSyncJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ErpStockSyncJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = configuration.GetValue("ErpSync:IntervalMinutes", 1);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));

        logger.LogInformation("ERP stock sync started. Runs every {Minutes} minute(s)", minutes);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

                var updated = await stockService.SyncFromErpAsync(stoppingToken);
                if (updated > 0)
                    logger.LogInformation("ERP sync finished: {Count} product(s) restocked", updated);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // One failed run must not kill the job
                logger.LogError(ex, "ERP stock sync failed");
            }
        }
    }
}
