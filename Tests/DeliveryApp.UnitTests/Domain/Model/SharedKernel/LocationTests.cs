using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class LocationTests
{
    [Fact]
    public void Create_LocationWithinBounds_ReturnsLocation()
    {
        // Arrange
        var x = 5;
        var y = 5;

        // Act
        var result = Location.Create(x, y);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(x, result.Value.X);
        Assert.Equal(y, result.Value.Y);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(11, 5)]
    public void Create_XOutsideBounds_ReturnsError(int x, int y)
    {
        // Arrange

        // Act
        var result = Location.Create(x, y);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsInvalid(nameof(x)), result.Error);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, 11)]
    public void Create_YOutsideBounds_ReturnsError(int x, int y)
    {
        // Arrange

        // Act
        var result = Location.Create(x, y);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GeneralErrors.ValueIsInvalid(nameof(y)), result.Error);
    }

    [Theory]
    [InlineData(2, 6, 4, 9, 5)]
    [InlineData(4, 9, 2, 6, 5)] // Верхнее, только назад
    [InlineData(1, 1, 1, 1, 0)]
    [InlineData(1, 1, 10, 10, 18)]
    public void DistanceTo_ReturnsCorrectDistance(int x1, int y1, int x2, int y2, int expectedDistance)
    {
        // Arrange
        var location1 = Location.Create(x1, y1).Value;
        var location2 = Location.Create(x2, y2).Value;

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        Assert.Equal(expectedDistance, distance);
    }

    [Fact]
    public void CreateRandom_Location_ReturnsLocationWithinBounds()
    {
        // Act
        var location = Location.CreateRandom();

        // Assert
        Assert.True(location.X is >= 1 and <= 10);
        Assert.True(location.Y is >= 1 and <= 10);
    }

    [Fact]
    public void Equals_LocationWithSameCoordinates_ReturnsTrue()
    {
        // Arrange
        var location1 = Location.Create(3, 4).Value;
        var location2 = Location.Create(3, 4).Value;

        // Act
        var result = location1.Equals(location2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_LocationWithDifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var location1 = Location.Create(3, 4).Value;
        var location2 = Location.Create(5, 6).Value;

        // Act
        var result = location1.Equals(location2);

        // Assert
        Assert.False(result);
    }
}