using FluentValidation;
using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Common.Extensions;
using SolidShop.Api.Data.Repositories;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Features.Products;

// Interface Segregation: only the admin panel uses this one
public interface IProductAdminService
{
    Task<ProductResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

// Dependency Inversion: everything comes through the constructor as abstractions.
// This class doesn't know it's SQLite, doesn't know how notifications are sent,
// and doesn't call DateTime.UtcNow directly.
public class ProductAdminService(
    IProductRepository repository,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator,
    IProductNotifier notifier,
    TimeProvider clock) : IProductAdminService
{
    public async Task<ProductResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await GetProductOrThrowAsync(id, ct);
        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var sku = request.Sku.NormalizeSku();
        if (await repository.SkuExistsAsync(sku, ct))
            throw new ConflictException($"SKU '{sku}' already exists.");

        var slug = request.Name.ToSlug();
        if (await repository.SlugExistsAsync(slug, excludeId: null, ct))
            throw new ConflictException($"A product with a similar name already exists (slug '{slug}').");

        var product = Product.Create(request.Name, request.Description, sku, request.Price, request.Stock, clock.GetUtcNow());

        repository.Add(product);
        await repository.SaveChangesAsync(ct);

        await notifier.ProductCreatedAsync(product, ct);

        return product.ToResponse();
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var product = await GetProductOrThrowAsync(id, ct);

        var slug = request.Name.ToSlug();
        if (await repository.SlugExistsAsync(slug, excludeId: id, ct))
            throw new ConflictException($"Another product already uses the slug '{slug}'.");

        product.Update(request.Name, request.Description, request.Price, clock.GetUtcNow());
        await repository.SaveChangesAsync(ct);

        return product.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await GetProductOrThrowAsync(id, ct);

        repository.Remove(product);
        await repository.SaveChangesAsync(ct);
    }

    private async Task<Product> GetProductOrThrowAsync(int id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");
}
