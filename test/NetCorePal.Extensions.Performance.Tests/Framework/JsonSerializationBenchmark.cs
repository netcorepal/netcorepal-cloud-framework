using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NetCorePal.Extensions.Performance.Tests.Framework;

[MemoryDiagnoser]
[SimpleJob]
public class JsonSerializationBenchmark
{
    private TestData _testData = null!;
    private string _systemTextJsonString = null!;
    private string _newtonsoftJsonString = null!;
    private JsonSerializerOptions _systemTextJsonOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testData = new TestData
        {
            Id = 12345,
            Name = "Test Entity",
            Description = "This is a test entity for JSON serialization performance testing",
            Value = 42.5m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Tags = new[] { "tag1", "tag2", "tag3", "performance", "test" },
            Metadata = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 },
                { "key3", true }
            }
        };

        _systemTextJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Pre-serialize for deserialization benchmarks
        _systemTextJsonString = JsonSerializer.Serialize(_testData, _systemTextJsonOptions);
        _newtonsoftJsonString = JsonConvert.SerializeObject(_testData);
    }

    [Benchmark(Baseline = true)]
    public string SystemTextJsonSerialize()
    {
        return JsonSerializer.Serialize(_testData, _systemTextJsonOptions);
    }

    [Benchmark]
    public string NewtonsoftJsonSerialize()
    {
        return JsonConvert.SerializeObject(_testData);
    }

    [Benchmark]
    public TestData SystemTextJsonDeserialize()
    {
        return JsonSerializer.Deserialize<TestData>(_systemTextJsonString, _systemTextJsonOptions)!;
    }

    [Benchmark]
    public TestData NewtonsoftJsonDeserialize()
    {
        return JsonConvert.DeserializeObject<TestData>(_newtonsoftJsonString)!;
    }

    [Benchmark]
    public List<TestData> SerializeMultipleSystemTextJson()
    {
        var list = new List<TestData>();
        for (int i = 0; i < 100; i++)
        {
            var data = new TestData
            {
                Id = i,
                Name = $"Entity {i}",
                Value = i * 1.5m,
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            list.Add(data);
        }
        
        var json = JsonSerializer.Serialize(list, _systemTextJsonOptions);
        return JsonSerializer.Deserialize<List<TestData>>(json, _systemTextJsonOptions)!;
    }

    [Benchmark]
    public List<TestData> SerializeMultipleNewtonsoftJson()
    {
        var list = new List<TestData>();
        for (int i = 0; i < 100; i++)
        {
            var data = new TestData
            {
                Id = i,
                Name = $"Entity {i}",
                Value = i * 1.5m,
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            list.Add(data);
        }
        
        var json = JsonConvert.SerializeObject(list);
        return JsonConvert.DeserializeObject<List<TestData>>(json)!;
    }
}

public class TestData
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string[]? Tags { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}