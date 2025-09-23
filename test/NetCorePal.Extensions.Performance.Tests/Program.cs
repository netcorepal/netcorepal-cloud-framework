using BenchmarkDotNet.Running;

namespace NetCorePal.Extensions.Performance.Tests;

public class Program
{
    public static void Main(string[] args)
    {
        var config = new CustomBenchmarkConfig();
        
        if (args.Length == 0)
        {
            // Run all benchmarks if no specific benchmark is specified
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
        else
        {
            // Run specific benchmark
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}