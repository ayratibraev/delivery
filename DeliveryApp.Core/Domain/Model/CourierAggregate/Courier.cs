using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier
{
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }

    private Courier(string name, Transport transport, Location location)
    {
        Id        = Guid.NewGuid();
        Name      = name;
        Transport = transport;
        Location  = location;
        Status    = CourierStatus.Free;
    }

    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Имя курьера
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Транспорт курьера
    /// </summary>
    public Transport Transport { get; private set; }

    /// <summary>
    ///     Местоположение курьера
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Статус курьера
    /// </summary>
    public CourierStatus Status { get; private set; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="transport">Транспорт</param>
    /// <param name="location">Местоположение</param>
    /// <returns>Результат</returns>
    public static Result<Courier, Error> Create(string name, Transport transport, Location location)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (transport == null) return GeneralErrors.ValueIsRequired(nameof(transport));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new Courier(name, transport, location);
    }

    /// <summary>
    ///     Установить статус Занят
    /// </summary>
    /// <returns></returns>
    public UnitResult<Error> SetBusy()
    {
        if (Status == CourierStatus.Busy) return Errors.AlreadyBusy();

        Status = CourierStatus.Busy;
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Установить статус Свободен
    /// </summary>
    /// <returns></returns>
    public UnitResult<Error> SetFree()
    {
        Status = CourierStatus.Free;
        return UnitResult.Success<Error>();
    }
    
    /// <summary>
    ///     Изменить местоположение
    /// </summary>
    /// <param name="targetLocation">Геопозиция</param>
    /// <returns>Результат</returns>
    public UnitResult<Error> Move(Location targetLocation)
    {
        if (targetLocation == null) return GeneralErrors.ValueIsRequired(nameof(targetLocation));

        var difX = targetLocation.X - Location.X;
        var difY = targetLocation.Y - Location.Y;

        var newX = Location.X;
        var newY = Location.Y;

        var cruisingRange = Transport.Speed;

        if (difX > 0)
        {
            if (difX >= cruisingRange)
            {
                newX += cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return UnitResult.Success<Error>();
            }

            if (difX < cruisingRange)
            {
                newX += difX;
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) 
                    return UnitResult.Success<Error>();
                cruisingRange -= difX;
            }
        }

        if (difX < 0)
        {
            if (Math.Abs(difX) >= cruisingRange)
            {
                newX -= cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return UnitResult.Success<Error>();
            }

            if (Math.Abs(difX) < cruisingRange)
            {
                newX -= Math.Abs(difX);
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation)
                    return UnitResult.Success<Error>();
                cruisingRange -= Math.Abs(difX);
            }
        }

        if (difY > 0)
        {
            if (difY >= cruisingRange)
            {
                newY += cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return UnitResult.Success<Error>();
            }

            if (difY < cruisingRange)
            {
                newY += difY;
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) 
                    return UnitResult.Success<Error>();
            }
        }

        if (difY < 0)
        {
            if (Math.Abs(difY) >= cruisingRange)
            {
                newY -= cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return UnitResult.Success<Error>();
            }

            if (Math.Abs(difY) < cruisingRange)
            {
                newY -= Math.Abs(difY);
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) 
                    return UnitResult.Success<Error>();
            }
        }

        Location = Location.Create(newX, newY).Value;
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Рассчитать время до точки
    /// </summary>
    /// <param name="location">Конечное местоположение</param>
    /// <returns>Результат</returns>
    public Result<double, Error> CalculateTimeToPoint(Location location)
    {
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var distance = Location.DistanceTo(location).Value;
        var time     = (double)distance / Transport.Speed;
        return time;
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error AlreadyBusy()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.already.busy",
                "Нельзя взять заказ в работу, курьер уже занят");
        }
    }
}