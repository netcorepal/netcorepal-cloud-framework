namespace NetCorePal.Extensions.Dto;

public interface IPageRequest
{
    /// <summary>
    /// 请求的页码，从1开始
    /// </summary>
    int PageIndex { get; set; }

    /// <summary>
    /// 请求的每页条数
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// 是否获取总数
    /// </summary>
    bool CountTotal { get; set; }
}