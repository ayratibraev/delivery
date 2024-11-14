using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier;

/// <summary>
///     Назначить заказ на курьера
/// </summary>
public class AssignOrderToCourierCommand : IRequest<bool>
{
    public AssignOrderToCourierCommand()
    {
    }
}