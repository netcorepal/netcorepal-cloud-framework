using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore
{
    public interface IPagedQuery<TResponse> : IQuery<PagedData<TResponse>>
    {
        public int PageIndex { get; }
        public int PageSize { get; }
        public bool CountTotal { get; }
    }
}
