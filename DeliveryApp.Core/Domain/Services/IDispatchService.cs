using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

/// <summary>
///     Диспетчеризация заказов-курьеров
/// </summary>
public interface IDispatchService
{
    /// <summary>
    ///     Метод делает скоринг и возвращает победившего курьера для этого заказа
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <param name="couriers">Список курьеров</param>
    /// <returns></returns>
    Result<Courier, Error> Dispatch(Order order, IEnumerable<Courier> couriers);
}