using Microsoft.EntityFrameworkCore;
using SolidShop.Api.Common.Extensions;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Description).HasMaxLength(2000);
            b.Property(p => p.Sku).HasMaxLength(50).IsRequired();
            b.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            b.Property(p => p.Price).HasPrecision(18, 2);
            b.HasIndex(p => p.Sku).IsUnique();
            b.HasIndex(p => p.Slug).IsUnique();

            b.HasData(SeedProducts());
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.PaymentMethod).HasMaxLength(50).IsRequired();
            b.Property(o => o.PaymentId).HasMaxLength(100);
            b.Property(o => o.Total).HasPrecision(18, 2);
            b.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);

            b.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // EF writes to the private _items field instead of the read-only property
            b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            b.Property(i => i.UnitPrice).HasPrecision(18, 2);
            b.Ignore(i => i.Subtotal);
        });
    }

    private static object[] SeedProducts()
    {
        var seedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        return
        [
            new
            {
                Id = 1, Name = "Zapatilla Running Niño", Slug = "Zapatilla Running Niño".ToSlug(),
                Description = "Zapatilla liviana con suela de goma antideslizante, ideal para uso diario y deportes.",
                Sku = "ZAP-RUN-KID-01", Price = 89999m, Stock = 25, CreatedAt = seedDate, UpdatedAt = (DateTimeOffset?)null
            },
            new
            {
                Id = 2, Name = "Remera Deportiva Dry-Fit", Slug = "Remera Deportiva Dry-Fit".ToSlug(),
                Description = "Remera de secado rápido con tejido respirable.",
                Sku = "REM-DRY-001", Price = 24999m, Stock = 100, CreatedAt = seedDate, UpdatedAt = (DateTimeOffset?)null
            },
            new
            {
                Id = 3, Name = "Mochila Urbana 20L", Slug = "Mochila Urbana 20L".ToSlug(),
                Description = "Mochila con compartimento acolchado para notebook de hasta 15 pulgadas.",
                Sku = "MOC-URB-20L", Price = 59999m, Stock = 8, CreatedAt = seedDate, UpdatedAt = (DateTimeOffset?)null
            },
            new
            {
                // Out of stock on purpose: the ERP sync job will restock it after a minute
                Id = 4, Name = "Botella Térmica 750ml", Slug = "Botella Térmica 750ml".ToSlug(),
                Description = "Acero inoxidable, mantiene frío 24 horas y caliente 12 horas.",
                Sku = "BOT-TER-750", Price = 32999m, Stock = 0, CreatedAt = seedDate, UpdatedAt = (DateTimeOffset?)null
            }
        ];
    }
}
