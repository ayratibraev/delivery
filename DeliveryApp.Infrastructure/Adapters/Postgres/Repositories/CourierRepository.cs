using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourierRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Add(Courier courier)
    {
        if (courier.Transport is not null) _dbContext.Attach(courier.Transport);
        if (courier.Status is not null) _dbContext.Attach(courier.Status);

        await _dbContext.Couriers.AddAsync(courier);
    }

    public void Update(Courier courier)
    {
        if (courier.Transport is not null) _dbContext.Attach(courier.Transport);
        if (courier.Status is not null) _dbContext.Attach(courier.Status);

        _dbContext.Couriers.Update(courier);
    }

    public Task<Courier> Get(Guid courierId) =>
        _dbContext
           .Couriers
           .Include(x => x.Transport)
           .Include(x => x.Status)
           .FirstOrDefaultAsync(o => o.Id == courierId);

    public IEnumerable<Courier> GetAllFree() =>
        _dbContext
           .Couriers
           .Include(x => x.Transport)
           .Include(x => x.Status)
           .Where(o => o.Status == CourierStatus.Free);
}