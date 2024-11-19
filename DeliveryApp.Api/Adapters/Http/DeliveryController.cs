using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Controllers;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Courier = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Courier;
using Location = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Location;
using Order = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Order;

namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController : DefaultApiController
{
    private readonly IMediator _mediator;

    public DeliveryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<IActionResult> CreateOrder()
    {
        var createOrderCommand = new CreateOrderCommand(Guid.NewGuid(), "street");
        var result = await _mediator.Send(createOrderCommand);
        if (result) return Ok();
        
        return Problem(statusCode: 500);
    }

    public override async Task<IActionResult> GetCouriers()
    {
        var getAllCouriersQuery = new GetCouriersQuery();
        var response            = await _mediator.Send(getAllCouriersQuery);

        var result = response.Couriers
           .Select(x => new Courier()
            {
                Id = x.Id,
                Name = x.Name,
                Location = new Location()
                {
                    X = x.Location.X,
                    Y = x.Location.Y
                }
            });
        
        return Ok(result);
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getActiveOrdersQuery = new GetCreatedAndAssignedOrdersQuery();
        var response             = await _mediator.Send(getActiveOrdersQuery);
        
        var result = response.Orders
           .Select(x => new Order()
            {
                Id   = x.Id,
                Location = new Location()
                {
                    X = x.Location.X,
                    Y = x.Location.Y
                }
            });
        
        return Ok(result);
    }
}