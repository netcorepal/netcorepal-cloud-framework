using System.ComponentModel.DataAnnotations;
using DotNetCore.CAP;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Application.IntegrationEventHandlers;
using NetCorePal.Web.Application.Queries;
using NetCorePal.Web.Application.Sagas;

namespace NetCorePal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IMediator mediator, OrderQuery orderQuery, ICapPublisher capPublisher, ISagaManager sagaManager) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [HttpPost]
        public async Task<OrderId> Post([FromBody] CreateOrderCommand command)
        {
            var id = await mediator.Send(command, HttpContext.RequestAborted);
            return id;
        }


        [HttpGet]
        [Route("/get/{id}")]
        public async Task<Order?> GetById([FromRoute] OrderId id)
        {
            var order = await orderQuery.QueryOrder(id, HttpContext.RequestAborted);
            return order;
        }





        [HttpGet]
        [Route("/sendEvent")]
        public async Task SendEvent(OrderId id)
        {
            await capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(id), cancellationToken: HttpContext.RequestAborted);
        }


        [HttpGet]
        [Route("/saga")]
        public async Task<ResponseData<long>> Saga()
        {
            return await sagaManager.SendAsync<CreateOrderSaga, CreateOrderSagaData, long>(new CreateOrderSagaData(), HttpContext.RequestAborted).AsResponseData();
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
        
        
        [HttpGet]
        [Route("/service/knownexception")]
        public Task<ResponseData<long>> ServiceKnownException()
        {
            throw new KnownException("test known exception message", 33);
        }
        
        
        [HttpGet]
        [Route("/service/unknownexception")]
        public Task<ResponseData<long>> ServiceUnknownException()
        {
            throw new Exception("系统异常");
        }
        
        [HttpGet]
        [Route("/badrequest/{id}")]
        public Task<ResponseData<long>> BadRequest(long id)
        {
            throw new Exception("系统异常");
        }

    }
}
