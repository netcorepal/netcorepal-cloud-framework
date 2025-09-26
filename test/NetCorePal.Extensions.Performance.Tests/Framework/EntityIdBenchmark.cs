using System.Text.Json;

namespace NetCorePal.Extensions.Performance.Tests.Framework;

[MemoryDiagnoser]
[SimpleJob]
public class StringOperationsBenchmark
{
    private string _testString = null!;
    private string[] _strings = null!;
    private List<string> _stringList = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testString = "This is a test string for performance benchmarking";
        _strings = Enumerable.Range(1, 1000).Select(i => $"String {i}").ToArray();
        _stringList = new List<string>(_strings);
    }

    [Benchmark(Baseline = true)]
    public string StringConcatenation()
    {
        var result = "";
        for (int i = 0; i < 100; i++)
        {
            result += $"Item {i} ";
        }
        return result;
    }

    [Benchmark]
    public string StringBuilderOperation()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            sb.Append($"Item {i} ");
        }
        return sb.ToString();
    }

    [Benchmark]
    public string StringJoin()
    {
        var items = Enumerable.Range(0, 100).Select(i => $"Item {i}");
        return string.Join(" ", items);
    }

    [Benchmark]
    public bool StringContains()
    {
        return _testString.Contains("performance");
    }

    [Benchmark]
    public string[] StringSplit()
    {
        return _testString.Split(' ');
    }

    [Benchmark]
    public string StringReplace()
    {
        return _testString.Replace("test", "performance");
    }

    [Benchmark]
    public string StringSubstring()
    {
        return _testString.Substring(10, 20);
    }

    [Benchmark]
    public List<string> StringListSearch()
    {
        return _stringList.Where(s => s.Contains("5")).ToList();
    }
}