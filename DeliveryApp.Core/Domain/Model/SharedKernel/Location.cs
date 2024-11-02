using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

/// <summary>
///     Координата
/// </summary>
public class Location : ValueObject
{
    /// <summary>
    ///     Минимальная координата
    /// </summary>
    public static readonly Location Min = new(1, 1);

    /// <summary>
    ///     Максимальная координата
    /// </summary>
    public static readonly Location Max = new(10, 10);


    [ExcludeFromCodeCoverage]
    private Location() { }
    
    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    /// <param name="x">Горизонталь</param>
    /// <param name="y">Вертикаль</param>
    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < Min.X || x > Max.X) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y < Min.Y || y > Max.Y) return GeneralErrors.ValueIsInvalid(nameof(y));

        return new Location(x, y);
    }

    /// <summary>
    ///     Посчитать расстояние между координатами
    /// </summary>
    public Result<int, Error> DistanceTo(Location targetLocation)
    {
        if (targetLocation is null) return GeneralErrors.ValueIsRequired(nameof(targetLocation));

        return Math.Abs(X - targetLocation.X) + Math.Abs(Y - targetLocation.Y);
    }

    /// <summary>
    ///     Сгенерировать случайную координату. Используется в целях тестирования
    /// </summary>
    public static Location CreateRandom()
    {
        return new Location(
            Random.Shared.Next(Min.X, Max.X),
            Random.Shared.Next(Min.Y, Max.Y));
    }

    /// <summary>
    ///     Перегрузка для определения идентичности
    /// </summary>
    /// <returns>Результат</returns>
    /// <remarks>Идентичность будет происходить по совокупности полей указанных в методе</remarks>
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}