using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SolidShop.Api.BackgroundJobs;
using SolidShop.Api.Common.Errors;
using SolidShop.Api.Data;
using SolidShop.Api.Data.Repositories;
using SolidShop.Api.Features.Orders;
using SolidShop.Api.Features.Payments;
using SolidShop.Api.Features.Payments.Providers;
using SolidShop.Api.Features.Products;

namespace SolidShop.Api;

// Extension methods over IServiceCollection keep Program.cs clean (Open/Closed + Dependency Inversion)
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // SCOPED: one instance per HTTP request.
        // They use repositories, which use the DbContext (also scoped).
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IProductAdminService, ProductAdminService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // TRANSIENT: stateless and cheap to create, a new instance every time.
        // Adding a new payment method = one new class + one line here (Open/Closed).
        services.AddTransient<IPaymentProvider, MercadoPagoProvider>();
        services.AddTransient<IPaymentProvider, StripeProvider>();
        services.AddTransient<IPaymentProvider, CashOnDeliveryProvider>();

        services.AddTransient<IProductNotifier, LoggingProductNotifier>();

        // FluentValidation registers validators as Scoped by default
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

        // SINGLETON: a single instance for the whole app. Must be thread-safe.
        // TimeProvider replaces DateTime.UtcNow so time can be faked in tests.
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // AddDbContext registers the DbContext as SCOPED
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Hosted services are SINGLETON -> they must create their own scope (see ErpStockSyncJob)
        services.AddHostedService<ErpStockSyncJob>();

        return services;
    }

    // Demo only: creates the SQLite database with seed data.
    // In production you would use EF Core migrations.
    public static WebApplication EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        return app;
    }
}
