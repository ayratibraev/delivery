using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public class Order : Aggregate
{
    [ExcludeFromCodeCoverage]
    private Order()
    {
    }

    public Order(Guid id, Location location) : base(id)
    {
        Id       = id;
        Location = location;
        Status   = OrderStatus.Created;
    }

    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Местоположение, куда нужно доставить заказ
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Идентификатор курьера
    /// </summary>
    public Guid? CourierId { get; private set; }

    /// <summary>
    ///     Статус заказа
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    ///     Создание заказа
    /// </summary>
    /// <param name="id">Айди корзины</param>
    /// <param name="location">Куда нужно доставить заказ</param>
    /// <returns></returns>
    public static Result<Order, Error> Create(Guid id, Location location)
    {
        if (id == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(id));
        if (location is null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new Order(id, location);
    }

    public UnitResult<Error> Assign(Courier courier)
    {
        if (courier is null) return GeneralErrors.ValueIsRequired(nameof(courier));
        if(courier.Status != CourierStatus.Free) return Errors.CantAssignOrderToBusyCourier(courier.Id);
        
        CourierId = courier.Id;
        Status    = OrderStatus.Assigned;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned) return Errors.CantCompleteNotAssignedOrder();
        
        Status = OrderStatus.Completed;
        
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error CantCompleteNotAssignedOrder()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.complete.not.assigned.order",
                "Нельзя завершить заказ, который не был назначен");
        }

        public static Error CantAssignOrderToBusyCourier(Guid courierId)
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.order.to.busy.courier",
                $"Нельзя назначить заказ на курьера, который занят. Id курьера = {courierId}");
        }
    }

}