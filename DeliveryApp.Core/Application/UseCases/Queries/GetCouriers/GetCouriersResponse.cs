namespace DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;

public class GetCouriersResponse
{
    public GetCouriersResponse(List<Courier> couriers)
    {
        Couriers = couriers;
    }

    public IReadOnlyCollection<Courier> Couriers { get; set; }
}

public class Courier
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    ///     Вид транспорта
    /// </summary>
    public int TransportId { get; set; }
}

public class Location
{
    /// <summary>
    ///     Горизонталь
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     Вертикаль
    /// </summary>
    public int Y { get; set; }
}