using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

public class DispatchService : IDispatchService
{
    /// <inheritdoc cref="IDispatchService" />
    public Result<Courier, Error> Dispatch(Order order, IEnumerable<Courier> couriers)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (couriers is null) return GeneralErrors.ValueIsRequired(nameof(couriers));

        var fastestCourier = couriers
           .Where(c => c.Status == CourierStatus.Free)
           .MinBy(c => c.CalculateTimeToPoint(order.Location).Value);

        if (fastestCourier is null) return Errors.CourierWasNotFound();

        // назначаем курьера на заказ
        var courierAssignToOrderResult = order.Assign(fastestCourier);
        if (courierAssignToOrderResult.IsFailure) return courierAssignToOrderResult.Error;

        // отмечаем курьера как Занят
        var courierSetBusyResult = fastestCourier.SetBusy();
        if (courierSetBusyResult.IsFailure) return courierSetBusyResult.Error;

        return fastestCourier;
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сервис
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error CourierWasNotFound()
        {
            return new Error("courier.was.not.found", "Подходящий курьер не был найден");
        }
    }
}