using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;

public class GetCouriersHandler : IRequestHandler<GetCouriersQuery, GetCouriersResponse>
{
    private readonly string _connectionString;

    public GetCouriersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<GetCouriersResponse> Handle(GetCouriersQuery message,
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
            FROM public.couriers";
        
        var couriers = await connection.QueryAsync<Courier, Location, Courier>(query, (courier, location) => {
                courier.Location = location;
                return courier;
            },
            new { },
            splitOn: "X" );

        return new GetCouriersResponse(couriers.ToList());
    }
}