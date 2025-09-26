using Microsoft.EntityFrameworkCore;

namespace NetCorePal.Extensions.Performance.Tests.Database;

[MemoryDiagnoser]
[SimpleJob]
public class EntityFrameworkBenchmark
{
    private IServiceProvider _serviceProvider = null!;
    private TestDbContext _dbContext = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}"));
        
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<TestDbContext>();

        // Seed some data
        var entities = new List<TestEntity>();
        for (int i = 1; i <= 1000; i++)
        {
            entities.Add(new TestEntity 
            { 
                Id = i, 
                Name = $"Entity {i}",
                Value = i * 10,
                IsActive = i % 2 == 0
            });
        }
        _dbContext.TestEntities.AddRange(entities);
        await _dbContext.SaveChangesAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<TestEntity?> GetByIdAsync()
    {
        return await _dbContext.TestEntities
            .FirstOrDefaultAsync(e => e.Id == 500);
    }

    [Benchmark]
    public async Task<TestEntity?> GetByIdNoTrackingAsync()
    {
        return await _dbContext.TestEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == 500);
    }

    [Benchmark]
    public async Task<List<TestEntity>> GetMultipleEntitiesAsync()
    {
        return await _dbContext.TestEntities
            .Where(e => e.Value > 500 && e.Value < 600)
            .ToListAsync();
    }

    [Benchmark]
    public async Task<TestEntity> AddEntityAsync()
    {
        var entity = new TestEntity 
        { 
            Id = Random.Shared.Next(10000, 20000),
            Name = "New Entity",
            Value = 999,
            IsActive = true
        };
        
        _dbContext.TestEntities.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    [Benchmark]
    public async Task<TestEntity> UpdateEntityAsync()
    {
        var entity = await _dbContext.TestEntities
            .FirstOrDefaultAsync(e => e.Id == 250);
        if (entity != null)
        {
            entity.Name = "Updated Entity";
            entity.Value = 9999;
            await _dbContext.SaveChangesAsync();
        }
        return entity!;
    }

    [Benchmark]
    public async Task<int> CountEntitiesAsync()
    {
        return await _dbContext.TestEntities
            .CountAsync(e => e.IsActive);
    }

    [Benchmark]
    public async Task<bool> ExistsCheckAsync()
    {
        return await _dbContext.TestEntities
            .AnyAsync(e => e.Id == 750);
    }
}

// Test DbContext
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
        });
    }
}

// Test Entity
public class TestEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Value { get; set; }
    public bool IsActive { get; set; }
}