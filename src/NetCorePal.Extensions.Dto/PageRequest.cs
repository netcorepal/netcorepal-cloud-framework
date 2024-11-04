namespace NetCorePal.Extensions.Dto;

/// <summary>
/// 分页请求模型
/// </summary>
public class PageRequest
{
    /// <summary>
    /// 请求的页码，从1开始
    /// </summary>
    public int? Index { get; set; }
    /// <summary>
    /// 请求的每页条数
    /// </summary>
    public int? Size { get; set; }
}
