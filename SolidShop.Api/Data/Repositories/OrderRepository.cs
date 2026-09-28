using Microsoft.EntityFrameworkCore;
using SolidShop.Api.Domain;

namespace SolidShop.Api.Data.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Order order);
    Task SaveChangesAsync(CancellationToken ct);
}

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public void Add(Order order) => db.Orders.Add(order);

    // Both repositories share the SAME DbContext during a request (Scoped).
    // So this single call saves the order AND the stock changes in one transaction.
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
