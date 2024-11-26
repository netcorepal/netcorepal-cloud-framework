using System.ComponentModel.DataAnnotations;

namespace NetCorePal.Extensions.Dto;

/// <summary>
/// 分页请求模型
/// </summary>
public class PageRequest : IPageRequest
{
    /// <summary>
    /// 请求的页码，从1开始
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; }

    /// <summary>
    /// 请求的每页条数
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = int.MaxValue;

    /// <summary>
    /// 是否获取总数
    /// </summary>
    public bool CountTotal { get; set; }
}