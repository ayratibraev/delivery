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
        
        var query =
            @"SELECT id, 
                    courier_id as CourierId,
                    status_id as StatusId,
                    location_x as X,
                    location_y as Y
             FROM public.orders where status_id!=@status_id;";
        
        var couriers = await connection.QueryAsync<Order, Location, Order>(query, (order, location) => {
                order.Location = location;
                return order;
            },
            new { status_id = OrderStatus.Completed.Id},
            splitOn: "X" );

        return new GetCreatedAndAssignedOrdersResponse(couriers.ToList());
    }
}