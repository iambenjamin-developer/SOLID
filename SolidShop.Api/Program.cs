using Scalar.AspNetCore;
using SolidShop.Api;

var builder = WebApplication.CreateBuilder(args);

// Each layer registers its own dependencies (see DependencyInjection.cs)
builder.Services
    .AddApiServices()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(); // uses GlobalExceptionHandler -> ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                    // /openapi/v1.json
    app.MapScalarApiReference(options => // /scalar
        options.WithTitle("SolidShop API"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.EnsureDatabaseCreated();

app.Run();
