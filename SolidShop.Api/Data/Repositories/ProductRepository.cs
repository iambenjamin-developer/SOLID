using Microsoft.EntityFrameworkCore;
using SolidShop.Api.Common;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
    Task<List<Product>> GetOutOfStockAsync(CancellationToken ct);
    Task<PagedResult<Product>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct);
    void Add(Product product);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken ct);
}

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    // Read-only query: AsNoTracking is faster because EF doesn't track changes
    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public Task<List<Product>> GetOutOfStockAsync(CancellationToken ct) =>
        db.Products.Where(p => p.Stock == 0).ToListAsync(ct);

    public async Task<PagedResult<Product>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.Name, pattern) || EF.Functions.Like(p.Sku, pattern));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.Sku == sku, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.Slug == slug && (excludeId == null || p.Id != excludeId), ct);

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
