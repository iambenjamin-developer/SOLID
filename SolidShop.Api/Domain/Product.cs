using SolidShop.Api.Common.Exceptions;
using SolidShop.Api.Common.Extensions;

namespace SolidShop.Api.Domain;

// The entity protects its own rules (e.g. stock can't go negative).
// Nobody outside can do "product.Stock = -5" because setters are private.
public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Product() { } // required by EF Core

    public static Product Create(string name, string? description, string sku, decimal price, int stock, DateTimeOffset now) =>
        new()
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Sku = sku.NormalizeSku(),
            Slug = name.ToSlug(),
            Price = price,
            Stock = stock,
            CreatedAt = now
        };

    public void Update(string name, string? description, decimal price, DateTimeOffset now)
    {
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Slug = name.ToSlug();
        Price = price;
        UpdatedAt = now;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than 0.");

        if (Stock < quantity)
            throw new BusinessRuleException($"Not enough stock for '{Name}'. Available: {Stock}, requested: {quantity}.");

        Stock -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than 0.");

        Stock += quantity;
    }

    public void SetStock(int stock, DateTimeOffset now)
    {
        if (stock < 0)
            throw new BusinessRuleException("Stock can't be negative.");

        Stock = stock;
        UpdatedAt = now;
    }
}
