using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public class Location : ValueObject
{
    [ExcludeFromCodeCoverage]
    private Location() { }
    
    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }
    
    public int X { get; }

    public int Y { get; }

    public static Result<Location, Error> Create(int x, int y)
    {
        if(x is < 1 or > 10) return GeneralErrors.ValueIsInvalid(nameof(x));
        if(y is < 1 or > 10) return GeneralErrors.ValueIsInvalid(nameof(y));
        
        return new Location(x, y);
    }
    
    public int DistanceTo(Location targetLocation) => Math.Abs(X - targetLocation.X) + Math.Abs(Y - targetLocation.Y);

    public static Location CreateRandom()
    {
        return new Location(
            Random.Shared.Next(1, 10),
            Random.Shared.Next(1, 10));
    }
    
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}