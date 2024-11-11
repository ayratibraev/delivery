using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Order order)
    {
        if (order.Status is not null) _dbContext.Attach(order.Status);

        await _dbContext.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        if (order.Status is not null) _dbContext.Attach(order.Status);

        _dbContext.Orders.Update(order);
    }

    public Task<Order> GetAsync(Guid orderId) =>
        _dbContext
           .Orders
           .Include(x => x.Status)
           .SingleOrDefaultAsync(o => o.Id == orderId);

    public Task<List<Order>> GetAllCreated() =>
        _dbContext
           .Orders
           .Include(x => x.Status)
           .Where(o => o.Status == OrderStatus.Created)
           .ToListAsync();

    public Task<List<Order>> GetAllAssigned() =>
        _dbContext
           .Orders
           .Include(x => x.Status)
           .Where(o => o.Status == OrderStatus.Assigned)
           .ToListAsync();
}