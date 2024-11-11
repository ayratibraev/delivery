using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;

public class GetCreatedAndAssignedOrdersHandler : IRequestHandler<GetCreatedAndAssignedOrdersQuery, GetCreatedAndAssignedOrdersResponse>
{
    private readonly string _connectionString;

    public GetCreatedAndAssignedOrdersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<GetCreatedAndAssignedOrdersResponse> Handle(GetCreatedAndAssignedOrdersQuery message,
                                                                  CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var result = await connection.QueryAsync<dynamic>(
            @"SELECT id, courier_id, location_x, location_y, status_id FROM public.orders where status_id!=@status_id;"
            , new { status_id = OrderStatus.Completed.Id });

        List<Order> orders = result
           .Select<dynamic, Order>(item => MapToOrder(item))
           .ToList();

        return new GetCreatedAndAssignedOrdersResponse(orders);
    }

    private Order MapToOrder(dynamic result)
    {
        var location = new Location { X = result.location_x, Y = result.location_y };
        var order    = new Order { Id   = result.id, Location  = location };
        return order;
    }

}