using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application.Commands.CreateOrder;

public class CreateOrderCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task ReturnTrueWhenOrderExists()
    {
        // Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value));
        
        _unitOfWorkMock.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        // Act
        var command = new CreateOrderCommand(Guid.NewGuid(), "street");
        var handler = new CreateOrderHandler(_unitOfWorkMock, _orderRepositoryMock);
        var result = await handler.Handle(command, new CancellationToken());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderCreatedSuccessfully()
    {
        // Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .ReturnsNull();
        
        _unitOfWorkMock.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        // Act
        var command = new CreateOrderCommand(Guid.NewGuid(), "street");
        var handler = new CreateOrderHandler(_unitOfWorkMock, _orderRepositoryMock);
        var result = await handler.Handle(command, new CancellationToken());

        // Assert
        await _orderRepositoryMock.Received(1).GetAsync(Arg.Any<Guid>());
        await _orderRepositoryMock.Received(1).AddAsync(Arg.Any<Order>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
        result.Should().BeTrue();
    }
}
