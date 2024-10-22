using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class TransportShould
{
    public static IEnumerable<object[]> GetTransports()
    {
        yield return [Transport.Pedestrian, 1, "pedestrian", 1];
        yield return [Transport.Bicycle, 2, "bicycle", 2];
        yield return [Transport.Car, 3, "car", 3];
    }

    [Theory]
    [MemberData(nameof(GetTransports))]
    public void ReturnCorrectProperties(Transport transport, int id, string name, int speed)
    {
        // Arrange

        // Act

        // Assert
        Assert.Equal(transport.Id, id);
        Assert.Equal(transport.Name, name);
        Assert.Equal(transport.Speed, speed);
    }
    
    [Theory]
    [MemberData(nameof(GetTransports))]
    public void CanBeFoundById(Transport transport, int id, string name, int speed)
    {
        //Arrange

        //Act
        var result = Transport.FromId(id).Value;

        //Assert
        Assert.True(transport == result);
        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(speed, result.Speed);
    }

    [Theory]
    [MemberData(nameof(GetTransports))]
    public void CanBeFoundByName(Transport transport, int id, string name, int speed)
    {
        //Arrange

        //Act
        var result = Transport.FromName(name).Value;
        
        //Assert
        Assert.True(transport == result);
        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(speed, result.Speed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(4)]
    public void ReturnErrorWhenInvalidId(int id)
    {
        // Arrange

        // Act
        var result = Transport.FromId(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Transport.Errors.InvalidId().Code, result.Error.Code);
        Assert.NotEqual(Transport.Errors.InvalidName().Code, result.Error.Code);
    }

    [Fact]
    public void ReturnErrorWhenInvalidName()
    {
        // Arrange
        var name = "velosiped";

        // Act
        var result = Transport.FromName(name);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Transport.Errors.InvalidName().Code, result.Error.Code);
        Assert.NotEqual(Transport.Errors.InvalidId().Code, result.Error.Code);
    }

    [Fact]
    public void SetName()
    {
        // Arrange
        var transport = Transport.Pedestrian;
        var oldName   = transport.Name;
        var newName   = "string.Empty";

        // Act
        transport.SetName(newName);

        // Assert
        Assert.Equal(newName, transport.Name);
        
        // Устанавливаем назад, чтобы не ломать тесты, потому что статическое
        transport.SetName(oldName);
    }
    
    [Fact]
    public void NotSetNameWhenInvalidName()
    {
        // Arrange
        var transport = Transport.Pedestrian;
        var newName   = string.Empty;

        // Act
        var result = transport.SetName(newName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired("name"), result.Error);
    }
    
    [Fact]
    public void SetSpeed()
    {
        // Arrange
        var transport = Transport.Pedestrian;
        var oldSpeed   = transport.Speed;
        var newSpeed   = 100;

        // Act
        transport.SetSpeed(newSpeed);

        // Assert
        Assert.Equal(newSpeed, transport.Speed);
        
        // Устанавливаем назад, чтобы не ломать тесты, потому что статическое
        transport.SetSpeed(oldSpeed);
    }

    [Fact]
    public void NotSetSpeedWhenInvalidSpeed()
    {
        // Arrange
        var transport = Transport.Pedestrian;
        var newSpeed  = 0;

        // Act
        var result = transport.SetSpeed(newSpeed);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsInvalid("speed"), result.Error);
    }
}