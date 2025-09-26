using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Performance.Tests.Framework;

[MemoryDiagnoser]
[SimpleJob]
public class DependencyInjectionBenchmark
{
    private IServiceProvider _serviceProvider = null!;
    private IServiceCollection _services = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceCollection();
        
        // Register common services like a typical application
        _services.AddLogging();
        _services.AddScoped<ITestService, TestService>();
        _services.AddScoped<ITestRepository, TestRepository>();
        _services.AddSingleton<ISingletonService, SingletonService>();
        _services.AddTransient<ITransientService, TransientService>();
        
        _serviceProvider = _services.BuildServiceProvider();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }

    [Benchmark(Baseline = true)]
    public ITestService ResolveScoped()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ITestService>();
    }

    [Benchmark]
    public ISingletonService ResolveSingleton()
    {
        return _serviceProvider.GetRequiredService<ISingletonService>();
    }

    [Benchmark]
    public ITransientService ResolveTransient()
    {
        return _serviceProvider.GetRequiredService<ITransientService>();
    }

    [Benchmark]
    public object ResolveMultipleServices()
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        return new
        {
            TestService = services.GetRequiredService<ITestService>(),
            Repository = services.GetRequiredService<ITestRepository>(),
            Singleton = services.GetRequiredService<ISingletonService>(),
            Transient = services.GetRequiredService<ITransientService>()
        };
    }
}

// Test interfaces and implementations
public interface ITestService
{
    string GetData();
}

public class TestService : ITestService
{
    public string GetData() => "Test Data";
}

public interface ITestRepository
{
    Task<TestEntity?> GetByIdAsync(long id);
}

public class TestRepository : ITestRepository
{
    public Task<TestEntity?> GetByIdAsync(long id)
    {
        return Task.FromResult<TestEntity?>(new TestEntity { Id = id });
    }
}

public interface ISingletonService
{
    DateTime GetTimestamp();
}

public class SingletonService : ISingletonService
{
    private readonly DateTime _timestamp = DateTime.UtcNow;
    public DateTime GetTimestamp() => _timestamp;
}

public interface ITransientService
{
    Guid GetId();
}

public class TransientService : ITransientService
{
    private readonly Guid _id = Guid.NewGuid();
    public Guid GetId() => _id;
}

public class TestEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}