﻿using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;

/// <summary>
///     Назначить заказ на курьера. Обработчик
///     Command: <see cref="AssignOrdersCommand"/>
/// </summary>
public class AssignOrdersHandler : IRequestHandler<AssignOrdersCommand, bool>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDispatchService _dispatchService;

    /// <summary>
    ///     Ctr
    /// </summary>
    public AssignOrdersHandler(
        IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        IUnitOfWork unitOfWork,
        IDispatchService dispatchService)
    {
        _orderRepository   = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
        _unitOfWork        = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dispatchService   = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
    }

    public async Task<bool> Handle(AssignOrdersCommand message, CancellationToken cancellationToken)
    {
        // Получаем агрегаты
        var orders = await _orderRepository.GetAllCreated();
        var order  = orders.FirstOrDefault();
        if (order == null) return false;
        
        var couriers = _courierRepository.GetAllFree().ToList();
        if (couriers.Count == 0) return false;

        // Распределяем заказы на курьеров
        var dispatchResult = _dispatchService.Dispatch(order, couriers);
        if (dispatchResult.IsFailure) return false;
        
        var courier = dispatchResult.Value;

        // Сохраняем агрегаты
        _courierRepository.Update(courier);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}