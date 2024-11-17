using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;

/// <summary>
///     Назначить заказ на курьера
///     Handler: <see cref="AssignOrdersHandler"/>
/// </summary>
public class AssignOrdersCommand : IRequest<bool>
{
    public AssignOrdersCommand()
    {
    }
}