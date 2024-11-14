using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

/// <summary>
///     Передвинуть курьеров
/// </summary>
public class MoveCouriersCommand : IRequest<bool>
{
    public MoveCouriersCommand()
    {
    }
}