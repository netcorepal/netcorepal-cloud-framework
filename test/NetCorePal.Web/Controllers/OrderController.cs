using DotNetCore.CAP;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Application.IntegrationEventHandlers;
using NetCorePal.Web.Application.Queries;
using NetCorePal.Web.Application.Sagas;
using NetCorePal.Web.Domain;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace NetCorePal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        readonly IMediator _mediator;
        readonly OrderQuery _orderQuery;
        readonly ICapPublisher _capPublisher;
        readonly ISagaManager _sagaManager;
        public OrderController(IMediator mediator, OrderQuery orderQuery, ICapPublisher capPublisher, ISagaManager sagaManager)
        {
            _mediator = mediator;
            _orderQuery = orderQuery;
            _capPublisher = capPublisher;
            _sagaManager = sagaManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [HttpPost]
        public async Task<OrderId> Post([FromBody] CreateOrderCommand command)
        {
            var id = await _mediator.Send(command);
            return id;
        }


        [HttpGet]
        [Route("/get/{id}")]
        public async Task<Order?> GetById([FromRoute] OrderId id)
        {
            var order = await _orderQuery.QueryOrder(id, HttpContext.RequestAborted);
            return order;
        }





        [HttpGet]
        [Route("/sendEvent")]
        public async Task SendEvent(OrderId id)
        {
            await _capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(id));
        }


        [HttpGet]
        [Route("/saga")]
        public async Task<ResponseData<long>> Saga()
        {
            return await _sagaManager.SendAsync<CreateOrderSaga, CreateOrderSagaData, long>(new CreateOrderSagaData(), HttpContext.RequestAborted).AsResponseData();
        }


        [HttpGet]
        [Route("/knownexception")]
        public Task<ResponseData<long>> KnownException()
        {
            throw new KnownException("test known exception message", 33);
        }


        [HttpGet]
        [Route("/unknownexception")]
        public Task<ResponseData<long>> UnknownException()
        {
            throw new Exception("系统异常");
        }

    }
}
