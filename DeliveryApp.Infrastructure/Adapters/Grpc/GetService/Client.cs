using DeliveryApp.Core.Ports;
using GeoApp.Api;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;

using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location; 

namespace DeliveryApp.Infrastructure.Adapters.Grpc.GetService;

public class GeoClient : IGeoClient
{
    private readonly string _serverUrl;
    private readonly SocketsHttpHandler _socketsHttpHandler;
    private readonly MethodConfig _methodConfig;
    
    public GeoClient(IOptions<Settings> options)
    {
        _serverUrl = options.Value.GeoServiceGrpcHost;
        
        _socketsHttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout    = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay             = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout           = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };

        _methodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts          = 5,
                InitialBackoff       = TimeSpan.FromSeconds(1),
                MaxBackoff           = TimeSpan.FromSeconds(5),
                BackoffMultiplier    = 1.5,
                RetryableStatusCodes = { StatusCode.Unavailable }
            }
        };
    }

    public async Task<Location> GetGeolocation(string street, CancellationToken cancellationToken)
    {
        using var channel = GrpcChannel.ForAddress(
            _serverUrl,
            new GrpcChannelOptions
            {
                HttpHandler   = _socketsHttpHandler,
                ServiceConfig = new ServiceConfig
                {
                    RetryThrottling = null,
                    MethodConfigs =
                    {
                        _methodConfig
                    }
                }
            });

        var client = new Geo.GeoClient(channel);
        var reply = await client.GetGeolocationAsync(new GetGeolocationRequest
        {
            Street = street
        }, null, DateTime.UtcNow.AddSeconds(2), cancellationToken);
        
        var locationResult = Location.Create(reply.Location.X, reply.Location.Y);
        if(locationResult.IsFailure) throw new Exception("Invalid location");
        
        return locationResult.Value;
    }
}