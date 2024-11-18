using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Controllers;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        return Ok(response.Couriers);
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getActiveOrdersQuery = new GetCreatedAndAssignedOrdersQuery();
        var response             = await _mediator.Send(getActiveOrdersQuery);
        return Ok(response.Orders);

    }
}