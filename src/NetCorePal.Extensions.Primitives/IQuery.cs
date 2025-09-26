using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Primitives
{


    /// <summary>
    /// 表示一个类为查询类，框架则会自动注册该类查询类
    /// </summary>
    public interface IQuery;


    /// <summary>
    /// 表示一个类为查询类，并指定查询结果类型，该Query由QueryHandler处理并相应
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IQuery<out TResponse> : IRequest<TResponse>;
}
