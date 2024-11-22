using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

/// <summary>
///     Создать заказ. Обработчик
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGeoClient _geoClient;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public CreateOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IGeoClient geoClient)
    {
        _unitOfWork      = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _geoClient  = geoClient;
    }

    public async Task<bool> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        // Если заказ уже существует, ничего не делаем 
        var order = await _orderRepository.GetAsync(message.BasketId);
        if (order != null) return true;

        var location = await _geoClient.GetGeolocation(message.Street, cancellationToken);
        
        // Создаем заказ
        var orderCreateResult = Order.Create(message.BasketId, location);
        if (orderCreateResult.IsFailure) return false;
        order = orderCreateResult.Value;

        // Сохраняем заказ
        await _orderRepository.AddAsync(order);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}