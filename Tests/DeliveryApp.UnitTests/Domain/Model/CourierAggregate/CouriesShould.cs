using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierShould
{
    #region Create
    
    [Fact]
    public void BeCreated_WithValidNameAndTransport()
    {
        // Arrange
        var name      = "John Doe";
        var transport = Transport.Bicycle;
        var location  = Location.CreateRandom();

        // Act
        var result = Courier.Create(name, transport, location);

        // Assert
        Assert.True(result.IsSuccess);

        var courier = result.Value;

        Assert.NotNull(courier);
        Assert.Equal(name, courier.Name);
        Assert.Equal(transport, courier.Transport);
        Assert.Equal(CourierStatus.Free, courier.Status);
    }

    [Fact]
    public void NotBeCreated_WithInvalidName_Null()
    {
        // Arrange
        string? name      = null;
        var transport = Transport.Bicycle;
        var location  = Location.CreateRandom();

        // Act
        var result = Courier.Create(name, transport, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(name)), result.Error);
    }

    [Fact]
    public void NotBeCreated_WithInvalidName_Empty()
    {
        // Arrange
        var name      = "";
        var transport = Transport.Bicycle;
        var location  = Location.CreateRandom();

        // Act
        var result = Courier.Create(name, transport, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(name)), result.Error);
    }

    [Fact]
    public void NotBeCreated_WithInvalidTransport()
    {
        // Arrange
        var        name      = "John Doe";
        Transport? transport = null;
        var        location  = Location.CreateRandom();

        // Act
        var result = Courier.Create(name, transport, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(transport)), result.Error);
    }
    
    [Fact]
    public void NotBeCreated_WithInvalidLocation()
    {
        // Arrange
        var name      = "John Doe";
        var transport = Transport.Car;
        var location  = Location.CreateRandom();

        // Act
        var result = Courier.Create(name, transport, location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(location)), result.Error);
    }
    
    #endregion

    [Fact]
    public void SetBusy_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.CreateRandom()).Value;

        // Act
        var result = courier.SetBusy();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CourierStatus.Busy, courier.Status);
    }
    
    [Fact]
    public void SetBusy_WhenAlreadyBusy_ReturnsError()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.CreateRandom()).Value;
        courier.SetBusy();

        // Act
        var result = courier.SetBusy();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Courier.Errors.AlreadyBusy(), result.Error);
    }

    [Fact]
    public void SetFree_WhenBusy_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.CreateRandom()).Value;
        courier.SetBusy();

        // Act
        var result = courier.SetFree();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CourierStatus.Free, courier.Status);
    }
    
    #region Move
    
    [Fact]
    public void NotMove_WhenTargetLocationIsNull()
    {
        // Arrange
        var       courier  = Courier.Create("John Doe", Transport.Bicycle, Location.CreateRandom()).Value;
        Location? location = null;
        
        // Act
        var result = courier.Move(location);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsRequired(nameof(location)), result.Error);
    }
    
    [Fact]
    public void Move_WhenTargetLocationIsSameAsCurrentLocation_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(10, 10).Value).Value;
        var targetLocation = Location.Create(10, 10).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(targetLocation, courier.Location);
    }

    [Fact]
    public void Move_WhenTargetLocationIsToTheRight_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(5, 10).Value).Value;
        var targetLocation = Location.Create(10, 10).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Location.Create(5 + Transport.Bicycle.Speed, 10).Value, courier.Location);
    }

    [Fact]
    public void Move_WhenTargetLocationIsToTheLeft_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(10, 10).Value).Value;
        var targetLocation = Location.Create(5, 10).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Location.Create(10 - Transport.Bicycle.Speed, 10).Value, courier.Location);
    }

    [Fact]
    public void Move_WhenTargetLocationIsAbove_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(10, 5).Value).Value;
        var targetLocation = Location.Create(10, 10).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Location.Create(10, 5 + Transport.Bicycle.Speed).Value, courier.Location);
    }

    [Fact]
    public void Move_WhenTargetLocationIsBelow_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(10, 10).Value).Value;
        var targetLocation = Location.Create(10, 5).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Location.Create(10, 10 - Transport.Bicycle.Speed).Value, courier.Location);
    }

    [Fact]
    public void Move_WhenDistanceToTargetLocationIsGreaterThanSpeed_ReturnsSuccess()
    {
        // Arrange
        var courier = Courier.Create("John Doe", Transport.Bicycle, Location.Create(5, 5).Value).Value;
        var targetLocation = Location.Create(15, 15).Value;

        // Act
        var result = courier.Move(targetLocation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Location.Create(5 + Transport.Bicycle.Speed, 5 + Transport.Bicycle.Speed).Value, courier.Location);
    }
    
    #endregion
}