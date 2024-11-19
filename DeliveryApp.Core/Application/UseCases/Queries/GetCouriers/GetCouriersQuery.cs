using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;

/// <summary>
///     Handler <see cref="GetCouriersHandler"/>>
/// </summary>
public class GetCouriersQuery : IRequest<GetCouriersResponse>;