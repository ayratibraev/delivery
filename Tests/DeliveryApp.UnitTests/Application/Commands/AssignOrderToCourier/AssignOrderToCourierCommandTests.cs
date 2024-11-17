using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application.Commands.AssignOrderToCourier;

public class AssignOrderToCourierCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepositoryMock = Substitute.For<ICourierRepository>();
    private readonly IDispatchService _dispatchServiceMock = Substitute.For<IDispatchService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private static readonly List<Order> RandomOrders = new()
    {
        Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value
    };

    private static readonly List<Courier> FreeCouriers = new List<Courier>()
    {
        Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value
    };
    
    [Fact]
    public async Task ReturnFalseWhenOrderNotExists()
    {
        // Arrange
        _orderRepositoryMock.GetAllCreated()
            .Returns(new List<Order>());

        // Act
        var command = new AssignOrdersCommand();
        var handler = new AssignOrdersHandler(_orderRepositoryMock, _courierRepositoryMock, _unitOfWorkMock, _dispatchServiceMock);
        var result  = await handler.Handle(command, new CancellationToken());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReturnFalseWhenOrderNoFreeCouriers()
    {
        // Arrange
        _orderRepositoryMock.GetAllCreated()
           .Returns(RandomOrders);

        _courierRepositoryMock.GetAllFree()
           .Returns([]);

        // Act
        var command = new AssignOrdersCommand();
        var handler = new AssignOrdersHandler(_orderRepositoryMock, _courierRepositoryMock, _unitOfWorkMock, _dispatchServiceMock);
        var result  = await handler.Handle(command, new CancellationToken());

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task ReturnFalseWhenDispatchFailed()
    {
        // Arrange
        _orderRepositoryMock.GetAllCreated()
           .Returns(RandomOrders);

        _courierRepositoryMock.GetAllFree()
           .Returns(FreeCouriers);

        _dispatchServiceMock.Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
           .Returns(Result.Failure<Courier, Error>(DispatchService.Errors.CourierWasNotFound()));

        // Act
        var command = new AssignOrdersCommand();
        var handler = new AssignOrdersHandler(_orderRepositoryMock, _courierRepositoryMock, _unitOfWorkMock, _dispatchServiceMock);
        var result  = await handler.Handle(command, new CancellationToken());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderCreatedSuccessfully()
    {
        // Arrange
        var order    = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        var orders   = new List<Order>() { order };
        var courier  = Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value;
        var couriers = new List<Courier>() { courier };
        
        _orderRepositoryMock.GetAllCreated()
           .Returns(orders);

        _courierRepositoryMock.GetAllFree()
           .Returns(couriers);

        _dispatchServiceMock.Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
           .Returns(courier);
        
        _unitOfWorkMock.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        // Act
        var command = new AssignOrdersCommand();
        var handler = new AssignOrdersHandler(_orderRepositoryMock, _courierRepositoryMock, _unitOfWorkMock, _dispatchServiceMock);
        var result  = await handler.Handle(command, new CancellationToken());

        // Assert
        _orderRepositoryMock.Received(1).GetAllCreated();
        _courierRepositoryMock.Received(1).GetAllFree();
        _dispatchServiceMock.Received(1).Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>());
        _orderRepositoryMock.Received(1).Update(order);
        _courierRepositoryMock.Received(1).Update(courier);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
        
        result.Should().BeTrue();
    }
}
