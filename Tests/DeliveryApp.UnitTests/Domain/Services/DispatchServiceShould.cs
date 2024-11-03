using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services;

public class DispatchServiceShould
{
    private readonly DispatchService _dispatchService = new();

    [Fact]
    public void ReturnError_NullOrder()
    {
        // Arrange
        Order? order = null;
        var couriers = new List<Courier>
        {
            Courier.Create("Aleksey", Transport.Car, Location.CreateRandom()).Value,
            Courier.Create("Lyoshsa", Transport.Bicycle, Location.CreateRandom()).Value,
            Courier.Create("Lyokha", Transport.Pedestrian, Location.CreateRandom()).Value
        };

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(order)), result.Error);
    }

    [Fact]
    public void ReturnError_NullCouriers()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        IEnumerable<Courier> couriers = null;

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(couriers)), result.Error);
    }

    [Fact]
    public void ReturnError_EmptyCouriers()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        IEnumerable<Courier> couriers = new List<Courier>
        {
            
        };

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DispatchService.Errors.CourierWasNotFound(), result.Error);
    }
    
    [Fact]
    public void ReturnError_HasNoFreeCouriers()
    {
        // Arrange
        var order       = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        var busyCourier = Courier.Create("Vasya", Transport.Car, Location.CreateRandom()).Value;
        busyCourier.SetBusy();
        IEnumerable<Courier> couriers = new List<Courier>
        {
            busyCourier
        };

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DispatchService.Errors.CourierWasNotFound(), result.Error);
    }

    [Fact]
    public void Dispatch_FreeCourierFound_Success()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        var freeCourier = Courier.Create("Vasya", Transport.Car, Location.CreateRandom()).Value;

        IEnumerable<Courier> couriers = new List<Courier>
        {
            freeCourier
        };

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(freeCourier, result.Value);
    }
    
    [Fact]
    public void Dispatch_FastestCourierFound_Success()
    {
        // Arrange
        var order  = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
        
        var slow   = Courier.Create("Vasya", Transport.Car, Location.Create(1, 1).Value).Value;
        var medium = Courier.Create("Igor", Transport.Bicycle, Location.Create(8, 8).Value).Value;
        var fast = Courier.Create("Yulia", Transport.Pedestrian, Location.Create(4, 4).Value).Value;

        IEnumerable<Courier> couriers = new List<Courier>
        {
            slow,
            fast,
            medium
        };

        // Act
        var result = _dispatchService.Dispatch(order, couriers);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(fast, result.Value);
    }
}