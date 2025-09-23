namespace NetCorePal.Extensions.Performance.Tests.Domain;

[MemoryDiagnoser]
[SimpleJob]
public class CollectionOperationsBenchmark
{
    private List<TestItem> _items = null!;
    private TestItem[] _itemArray = null!;
    private Dictionary<long, TestItem> _itemDictionary = null!;
    private HashSet<long> _itemHashSet = null!;

    [GlobalSetup]
    public void Setup()
    {
        _items = new List<TestItem>();
        for (int i = 1; i <= 10000; i++)
        {
            _items.Add(new TestItem { Id = i, Name = $"Item {i}", Value = i * 10 });
        }
        
        _itemArray = _items.ToArray();
        _itemDictionary = _items.ToDictionary(x => x.Id);
        _itemHashSet = _items.Select(x => x.Id).ToHashSet();
    }

    [Benchmark(Baseline = true)]
    public TestItem? ListLinearSearch()
    {
        return _items.FirstOrDefault(x => x.Id == 5000);
    }

    [Benchmark]
    public TestItem? ArrayLinearSearch()
    {
        return _itemArray.FirstOrDefault(x => x.Id == 5000);
    }

    [Benchmark]
    public TestItem? DictionaryLookup()
    {
        return _itemDictionary.GetValueOrDefault(5000);
    }

    [Benchmark]
    public bool HashSetContains()
    {
        return _itemHashSet.Contains(5000);
    }

    [Benchmark]
    public List<TestItem> ListWhere()
    {
        return _items.Where(x => x.Value > 50000).ToList();
    }

    [Benchmark]
    public TestItem[] ArrayWhere()
    {
        return _itemArray.Where(x => x.Value > 50000).ToArray();
    }

    [Benchmark]
    public int ListCount()
    {
        return _items.Count(x => x.Value > 50000);
    }

    [Benchmark]
    public TestItem? ListMax()
    {
        return _items.MaxBy(x => x.Value);
    }

    [Benchmark]
    public IEnumerable<TestItem> ListOrderBy()
    {
        return _items.OrderBy(x => x.Name);
    }

    [Benchmark]
    public ILookup<int, TestItem> ListGroupBy()
    {
        return _items.ToLookup(x => (int)(x.Value / 1000));
    }
}

public class TestItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Value { get; set; }
}