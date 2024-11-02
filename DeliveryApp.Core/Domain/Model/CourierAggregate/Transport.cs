using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Transport : Entity<int>
{
    public static readonly Transport Pedestrian = new(1, nameof(Pedestrian).ToLowerInvariant(), 1);
    public static readonly Transport Bicycle = new(2, nameof(Bicycle).ToLowerInvariant(), 2);
    public static readonly Transport Car = new(3, nameof(Car).ToLowerInvariant(), 3);
    
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Transport()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="id">ИД</param>
    /// <param name="name">Название</param>
    /// <param name="speed">Скорость</param>
    private Transport(int id, string name, int speed) : this()
    {
        Id    = id;
        Name  = name;
        Speed = speed;
    }
    
    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    ///     Скорость
    /// </summary>
    public int Speed { get; private set; }
    
    /// <summary>
    ///     Список всех значений списка
    /// </summary>
    /// <returns>Значения списка</returns>
    public static IReadOnlyCollection<Transport> List()
    {
        return new Transport[] { Pedestrian, Bicycle, Car };
    }

    /// <summary>
    ///     Изменить название
    /// </summary>
    /// <param name="name">Название</param>
    /// <returns>Результат</returns>
    public UnitResult<Error> SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));

        Name = name;
        return UnitResult.Success<Error>();
    }
    
    /// <summary>
    ///     Изменить скорость
    /// </summary>
    /// <param name="speed">Скорость</param>
    /// <returns>Результат</returns>
    public UnitResult<Error> SetSpeed(int speed)
    {
        if (speed <= 0) return GeneralErrors.ValueIsInvalid(nameof(speed));

        Speed = speed;
        return UnitResult.Success<Error>();
    }
    
    public static Result<Transport, Error> FromId(int id)
    {
        var transport = List()
           .SingleOrDefault(t => t.Id == id);

        if (transport == null) return Errors.InvalidId();

        return transport;
    }

    public static Result<Transport, Error> FromName(string name)
    {
        var transport = List()
           .SingleOrDefault(t => 
                t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

        if (transport == null) return Errors.InvalidName();

        return transport;
    }

    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        /// <summary>
        ///     Неверное значение Названия
        /// </summary>
        /// <returns></returns>
        public static Error InvalidName()
        {
            return new Error($"name.of.{nameof(Transport).ToLowerInvariant()}.is.invalid",
                $"Не верное значение. Допустимые значения: {nameof(Transport).ToLowerInvariant()}: {string.Join(",", List().Select(t => t.Name))}");
        }
        
        /// <summary>
        ///     Неверное значение ИД
        /// </summary>
        /// <returns></returns>
        public static Error InvalidId()
        {
            return new Error($"id.of.{nameof(Transport).ToLowerInvariant()}.is.invalid",
                $"Не верное значение. Допустимые значения: {nameof(Transport).ToLowerInvariant()}: {string.Join(",", List().Select(t => t.Id))}");
        }

    }
}