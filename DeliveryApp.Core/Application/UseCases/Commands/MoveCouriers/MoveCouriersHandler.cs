using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

public class MoveCouriersHandler : IRequestHandler<MoveCouriersCommand, bool>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public MoveCouriersHandler(
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository,
        ICourierRepository courierRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
    }

    public async Task<bool> Handle(MoveCouriersCommand message, CancellationToken cancellationToken)
    {
        // Получаем агрегаты
        var assignedOrders = await _orderRepository.GetAllAssigned();
        if (assignedOrders.Count == 0)
            return false;

        foreach (var order in assignedOrders)
        {
            var courier = await _courierRepository.GetAsync(order.CourierId!.Value);

            // Перемещаем курьера
            var courierMoveResult = courier.Move(order.Location);
            if (courierMoveResult.IsFailure) return false;

            // Если курьер дошел до точки заказа - завершаем заказ, освобождаем курьера
            if (order.Location == courier.Location)
            {
                order.Complete();
                courier.SetFree();
            }

            _courierRepository.Update(courier);
            _orderRepository.Update(order);
        }
        
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
