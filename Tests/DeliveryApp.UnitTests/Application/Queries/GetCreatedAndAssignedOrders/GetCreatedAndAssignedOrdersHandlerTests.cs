using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Courier = DeliveryApp.Core.Domain.Model.CourierAggregate.Courier;
using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.UnitTests.Application.Queries.GetCreatedAndAssignedOrders;

public class GetBusyCouriersHandlerShould : IAsyncLifetime
{
    /// <summary>
    ///     Настройка Postgres из библиотеки TestContainers
    /// </summary>
    /// <remarks>По сути это Docker контейнер с Postgres</remarks>
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
       .WithImage("postgres")
       .WithDatabase("couriers")
       .WithUsername("postgres")
       .WithPassword("postgres")
       .WithCleanUp(true)
       .Build();
    
    private ApplicationDbContext _context;

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
    public GetBusyCouriersHandlerShould()
    {
    }
    
    /// <summary>
    ///     Инициализируем окружение
    /// </summary>
    /// <remarks>Вызывается перед каждым тестом</remarks>
    public async Task InitializeAsync()
    {
        //Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
        await _postgreSqlContainer.StartAsync();

        //Накатываем миграции и справочники
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
           .Options;
        _context = new ApplicationDbContext(contextOptions);
        await _context.Database.MigrateAsync();
    }

    /// <summary>
    ///     Уничтожаем окружение
    /// </summary>
    /// <remarks>Вызывается после каждого теста</remarks>
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CanReturnBusyCouriers()
    {
        //Arrange
        var busyCourier = Courier.Create("Иван", Transport.Car, Location.Create(3, 5).Value).Value;
        busyCourier.SetBusy();

        var freeCourier = Courier.Create("Борис", Transport.Pedestrian, Location.Create(1, 1).Value).Value;
        
        var courierRepository = new CourierRepository(_context);
        var unitOfWork        = new UnitOfWork(_context);
        await courierRepository.AddAsync(busyCourier);
        await courierRepository.AddAsync(freeCourier);
        await unitOfWork.SaveChangesAsync();

        //Act
        var query   = new GetCouriersQuery();
        var handler = new GetCouriersHandler(_postgreSqlContainer.GetConnectionString());
        var result  = await handler.Handle(query, new CancellationToken());

        //Assert
        Assert.Single(result.Couriers);

        var courier = result.Couriers.First();

        Assert.Equal(busyCourier.Id, courier.Id);
        Assert.Equal(busyCourier.Name, courier.Name);
        Assert.Equal(busyCourier.Location.X, courier.Location.X);
        Assert.Equal(busyCourier.Location.Y, courier.Location.Y);
        Assert.Equal(busyCourier.Transport.Id, courier.TransportId);
    }
}