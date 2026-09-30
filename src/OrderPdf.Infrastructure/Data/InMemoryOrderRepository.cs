using System.Collections.Concurrent;
using OrderPdf.Application.Interfaces;
using OrderPdf.Domain.Entities;

namespace OrderPdf.Infrastructure.Data;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, Order> _orders = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryOrderRepository()
    {
        // Seed default sample orders
        foreach (var order in SampleDataSeeder.GetSampleOrders())
        {
            _orders[order.Id] = order;
        }
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Order> list = _orders.Values.OrderBy(o => o.OrderDate).ToList();
        return Task.FromResult(list);
    }

    public Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var order = _orders.Values.FirstOrDefault(o =>
            string.Equals(o.OrderNumber, orderNumber, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(order);
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        cancellationToken.ThrowIfCancellationRequested();
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
