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
        
        var result = await connection.QueryAsync(
            @"SELECT id, name, location_x, location_y, status_id, transport_id FROM public.couriers where status_id=@status_id"
            , new { status_id = CourierStatus.Busy.Id});

        List<Courier> couriers = result
           .Select<dynamic, Courier>(item => MapToCourier(item))
           .ToList();

        return new GetBusyCouriersResponse(couriers);
    }

    private Courier MapToCourier(dynamic result)
    {
        var location = new Location { X = result.location_x, Y = result.location_y };
        var courier = new Courier
            { Id = result.id, Name = result.name, Location = location, TransportId = result.transport_id };
        return courier;
    }
}