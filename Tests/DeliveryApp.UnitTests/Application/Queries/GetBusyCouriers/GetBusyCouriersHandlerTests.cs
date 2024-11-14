using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Courier = DeliveryApp.Core.Domain.Model.CourierAggregate.Courier;
using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location;
using Order = DeliveryApp.Core.Domain.Model.OrderAggregate.Order;

namespace DeliveryApp.UnitTests.Application.Queries.GetBusyCouriers;

public class GetCreatedAndAssignedOrders : IAsyncLifetime
{
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
    public GetCreatedAndAssignedOrders()
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
        var createdOrder  = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        var assignedOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        assignedOrder.Assign(Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value);
        
        var completedOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        completedOrder.Assign(Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value);
        completedOrder.Complete();
        
        var orderRepository = new OrderRepository(_context);
        var unitOfWork        = new UnitOfWork(_context);
        await orderRepository.AddAsync(createdOrder);
        await orderRepository.AddAsync(assignedOrder);
        await orderRepository.AddAsync(completedOrder);
        await unitOfWork.SaveChangesAsync();

        //Act
        var query   = new GetCreatedAndAssignedOrdersQuery();
        var handler = new GetCreatedAndAssignedOrdersHandler(_postgreSqlContainer.GetConnectionString());
        var result  = await handler.Handle(query, new CancellationToken());

        //Assert
        Assert.Equal(2, result.Orders.Count);
        Assert.Collection<DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders.Order>
            (result.Orders, 
                order =>
                {
                    Assert.True(order.Id == createdOrder.Id || order.Id == assignedOrder.Id);
                },
                order =>
                {
                    Assert.True(order.Id == createdOrder.Id || order.Id == assignedOrder.Id);
                });
    }
}