using Dapper;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;

public class GetBusyCouriersHandler : IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>
{
    private readonly string _connectionString;

    public GetBusyCouriersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<GetBusyCouriersResponse> Handle(GetBusyCouriersQuery message,
                                                      CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        var query =
            @"SELECT id as Id,
                name as Name,
                status_id as StatusId,
                transport_id as TransportId,
                location_x as X,
                location_y as Y
            FROM public.couriers where status_id=@status_id;";
        
        var couriers = await connection.QueryAsync<Courier, Location, Courier>(query, (courier, location) => {
                courier.Location = location;
                return courier;
            },
            new { status_id = CourierStatus.Busy.Id},
            splitOn: "X" );

        return new GetBusyCouriersResponse(couriers.ToList());
    }
}