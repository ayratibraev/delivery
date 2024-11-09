using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrderRepositoryShould : IAsyncLifetime
{
    private readonly Location _location;

    /// <summary>
    ///     Настройка Postgres из библиотеки TestContainers
    /// </summary>
    /// <remarks>По сути это Docker контейнер с Postgres</remarks>
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("orders")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private ApplicationDbContext _context;

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
    public OrderRepositoryShould()
    {
        var locationCreateResult = Location.Create(1, 1);
        locationCreateResult.IsSuccess.Should().BeTrue();
        _location = locationCreateResult.Value;
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
        _context.Database.Migrate();
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
    public async Task CanAddOrder()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, _location).Value;

        //Act
        var orderRepository = new OrderRepository(_context);
        var unitOfWork = new UnitOfWork(_context);

        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var orderFromDb = await orderRepository.GetAsync(order.Id);
        order.Should().BeEquivalentTo(orderFromDb);
    }

    [Fact]
    public async Task CanUpdateOrder()
    {
        //Arrange
        var courier = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value).Value;

        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, _location).Value;

        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        //Act
        var orderAssignToCourierResult = order.Assign(courier);
        orderAssignToCourierResult.IsSuccess.Should().BeTrue();
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var orderFromDb = await orderRepository.GetAsync(order.Id);
        order.Should().BeEquivalentTo(orderFromDb);
    }

    [Fact]
    public async Task CanGetById()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, _location).Value;

        //Act
        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var orderFromDb = await orderRepository.GetAsync(order.Id);
        order.Should().BeEquivalentTo(orderFromDb);
    }

    [Fact]
    public async Task CanGetAllFree()
    {
        //Arrange
        var courier = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value).Value;

        var order1Id = Guid.NewGuid();
        var order1 = Order.Create(order1Id, _location).Value;
        var orderAssignToCourierResult = order1.Assign(courier);
        orderAssignToCourierResult.IsSuccess.Should().BeTrue();

        var order2Id = Guid.NewGuid();
        var order2 = Order.Create(order2Id, _location).Value;

        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        //Act
        var activeOrdersFromDb = orderRepository.GetAllCreated();

        //Assert
        var ordersFromDb = activeOrdersFromDb.ToList();
        ordersFromDb.Should().NotBeEmpty();
        ordersFromDb.Count().Should().Be(1);
        ordersFromDb.First().Should().BeEquivalentTo(order2);
    }
}
