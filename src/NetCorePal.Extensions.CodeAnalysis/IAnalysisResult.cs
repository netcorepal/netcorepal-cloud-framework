namespace NetCorePal.Extensions.CodeAnalysis;
/// <summary>
/// 分析结果接口
/// </summary>
public interface IAnalysisResult
{
    /// <summary>
    /// 返回代码流分析结果
    /// </summary>
    /// <returns></returns>
    CodeFlowAnalysisResult GetResult();
}
