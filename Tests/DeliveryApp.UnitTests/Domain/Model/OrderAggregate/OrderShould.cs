using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate;

public class OrderShould
{
    [Fact]
    public void BeCreated_WithValidIdAndLocation_ReturnsOrder()
    {
        // Arrange
        var id       = Guid.NewGuid();
        var location = Location.Create(5, 5).Value;

        // Act
        var result = Order.Create(id, location);

        // Assert
        Assert.True(result.IsSuccess);

        var order = result.Value;

        Assert.NotNull(order);
        Assert.Equal(id, order.Id);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(location.X, order.Location.X);
        Assert.Equal(location.Y, order.Location.Y);
    }

    [Fact]
    public void NotBeCreated_WithEmptyId_ReturnsError()
    {
        // Arrange
        var id       = Guid.Empty;
        var location = Location.Create(5, 5).Value;

        // Act
        var result = Order.Create(id, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(id)), result.Error);
    }

    [Fact]
    public void NotBeCreated_OrderWithNullLocation_ReturnsError()
    {
        // Arrange
        var       id       = Guid.NewGuid();
        Location? location = null;

        // Act
        var result = Order.Create(id, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(location)), result.Error);
    }

    [Fact]
    public void BeAssigned_WithValidCourier_ReturnsSuccess()
    {
        // Arrange
        var order   = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
        var courier = Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value;

        // Act
        var result = order.Assign(courier);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Assigned, order.Status);
        Assert.Equal(courier.Id, order.CourierId);
    }

    [Fact]
    public void NotBeAssigned_WithNullCourier_ReturnsError()
    {
        // Arrange
        var      order   = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
        Courier? courier = null;

        // Act
        var result = order.Assign(courier);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(courier)), result.Error);
    }

    [Fact]
    public void NotBeAssigned_WithBusyCourier_ReturnsError()
    {
        // Arrange
        var order   = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
        var courier = Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value;
        courier.SetBusy();

        // Act
        var result = order.Assign(courier);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Order.Errors.CantAssignOrderToBusyCourier(courier.Id), result.Error);
    }


    [Fact]
    public void BeCompleted_WithAssignedStatus_ReturnsSuccess()
    {
        // Arrange
        var order   = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
        var courier = Courier.Create("Vasya", Transport.Bicycle, Location.CreateRandom()).Value;
        order.Assign(courier);

        // Act
        var result = order.Complete();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void NotBeCompleted_WithNotAssignedStatus_ReturnsError()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;

        // Act
        var result = order.Complete();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Order.Errors.CantCompleteNotAssignedOrder(), result.Error);
    }
}