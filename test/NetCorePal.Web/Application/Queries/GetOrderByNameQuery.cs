using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Queries
{


    public record GetOrderByNameDto(OrderId OrderId, string Name);

    public record GetOrderByNameQuery(string Name, int PageIndex = 1, int PageSize = 10, bool CountTotal = false) : IPagedQuery<GetOrderByNameDto>;


    public class GetOrderByNameQueryHandler(ApplicationDbContext dbContext) : IQueryHandler<GetOrderByNameQuery, PagedData<GetOrderByNameDto>>
    {
        public async Task<PagedData<GetOrderByNameDto>> Handle(GetOrderByNameQuery request, CancellationToken cancellationToken)
        {
            return await dbContext.Orders.Where(x => x.Name.Contains(request.Name))
               .Select(p => new GetOrderByNameDto(p.Id, p.Name))
               .ToPagedDataAsync(request, cancellationToken);
        }
    }
}
