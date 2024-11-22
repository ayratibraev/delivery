using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.Core.Ports;

public interface IGeoClient
{
    /// <summary>
    ///     Получить информацию о геолокации по улице
    /// </summary>
    Task<Location>  GetGeolocation(string street, CancellationToken cancellationToken);
}