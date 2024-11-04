using System.ComponentModel.DataAnnotations;
using DotNetCore.CAP;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using NetCorePal.Extensions.AspNetCore.Pagination;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Application.IntegrationEventHandlers;
using NetCorePal.Web.Application.Queries;
using NetCorePal.Web.Application.Sagas;
using NetCorePal.Web.Controllers.Request;
using SkyApm.Tracing;

namespace NetCorePal.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="orderQuery"></param>
    /// <param name="capPublisher"></param>
    /// <param name="sagaManager"></param>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(
        IMediator mediator,
        OrderQuery orderQuery,
        ICapPublisher capPublisher,
        ISagaManager sagaManager) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<OrderId> Post([FromBody] CreateOrderRequest request)
        {
            var id = await mediator.Send(new CreateOrderCommand(request.Name, request.Price, request.Count), HttpContext.RequestAborted);
            return id;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/get/{id}")]
        public async Task<OrderQueryResult?> GetById([FromRoute] OrderId id)
        {
            var order = await orderQuery.QueryOrder(id, HttpContext.RequestAborted);
            return order;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/list")]
        public async Task<ResponseData<PagedData<OrderQueryResult>>> ListByPage([FromQuery] ListOrdersRequest request)
        {
            var orders = await orderQuery.ListOrderByPage(request.Name, request.Index, request.Size, request.CountTotal, HttpContext.RequestAborted);
            return orders.AsResponseData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/setpaid")]
        public async Task<ResponseData> SetPaid(OrderId id)
        {
            await mediator.Send(new OrderPaidCommand(id), HttpContext.RequestAborted);
            return true.AsResponseData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/setorderItemName")]
        public async Task<ResponseData> SetOrderItemName([FromQuery] long id, [FromQuery] string name)
        {
            await mediator.Send(new SetOrderItemNameCommand(new OrderId(id), name), HttpContext.RequestAborted);
            return true.AsResponseData();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="_carrierPropagator"></param>
        [HttpGet]
        [Route("/sendEvent")]
        public async Task SendEvent(OrderId id, [FromServices] ICarrierPropagator _carrierPropagator)
        {
            await capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(id),
                cancellationToken: HttpContext.RequestAborted);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/saga")]
        public async Task<ResponseData<long>> Saga()
        {
            return await sagaManager
                .SendAsync<CreateOrderSaga, CreateOrderSagaData, long>(new CreateOrderSagaData(),
                    HttpContext.RequestAborted).AsResponseData();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpGet]
        [Route("/knownexception")]
        public Task<ResponseData<long>> KnownException()
        {
            throw new KnownException("test known exception message", 33);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("/unknownexception")]
        public Task<ResponseData<long>> UnknownException()
        {
            throw new Exception("系统异常");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpGet]
        [Route("/service/knownexception")]
        public Task<ResponseData<long>> ServiceKnownException()
        {
            throw new KnownException("test known exception message", 33);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("/service/unknownexception")]
        public Task<ResponseData<long>> ServiceUnknownException()
        {
            throw new Exception("系统异常");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("/badrequest/{id}")]
        public Task<ResponseData<long>> BadRequest(long id)
        {
            throw new Exception("系统异常");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/path/{id}")]
        public Task<ResponseData<OrderId>> Path([FromRoute] OrderId id)
        {
            return Task.FromResult(new ResponseData<OrderId>(id));
        }
    }

    /// <summary>
    /// 查询合同列表请求
    /// </summary>
    public class ListOrdersRequest : PageRequest
    {
        public string? Name { get; set; }
        public bool CountTotal { get; set; } = false;
    }
}