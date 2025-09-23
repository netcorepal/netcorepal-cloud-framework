using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Reports;

namespace NetCorePal.Extensions.Performance.Tests;

public class CustomBenchmarkConfig : ManualConfig
{
    public CustomBenchmarkConfig()
    {
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core80)
            .WithWarmupCount(3)
            .WithIterationCount(10)
            .WithMinIterationCount(5)
            .WithMaxIterationCount(20));

        AddDiagnoser(MemoryDiagnoser.Default);
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(HtmlExporter.Default);
        
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Median);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.P95);
        AddColumn(BaselineColumn.Default);
        
        SummaryStyle = SummaryStyle.Default;
    }
}