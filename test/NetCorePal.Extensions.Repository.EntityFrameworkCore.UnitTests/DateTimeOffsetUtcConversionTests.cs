using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;

/// <summary>
/// 仅用于 DateTimeOffset UTC 补丁测试的实体：包含 DateTimeOffset 与 DateTimeOffset? 属性。
/// </summary>
public class EntityWithDateTimeOffset
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? OptionalAt { get; set; }
}

/// <summary>
/// 仅用于 DateTimeOffset UTC 补丁测试的 DbContext（启用补丁时使用，独立类型避免 EF 模型缓存串用）。
/// </summary>
public partial class DateTimeOffsetEnabledTestDbContext : AppDbContextBase
{
    public DateTimeOffsetEnabledTestDbContext(DbContextOptions<DateTimeOffsetEnabledTestDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    public DbSet<EntityWithDateTimeOffset> EntitiesWithDate => Set<EntityWithDateTimeOffset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithDateTimeOffset>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// 仅用于“未调用扩展”场景的测试 DbContext（独立类型避免模型缓存串用）。
/// </summary>
public partial class DateTimeOffsetDisabledTestDbContext : AppDbContextBase
{
    public DateTimeOffsetDisabledTestDbContext(DbContextOptions<DateTimeOffsetDisabledTestDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    public DbSet<EntityWithDateTimeOffset> EntitiesWithDate => Set<EntityWithDateTimeOffset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithDateTimeOffset>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// UseDateTimeOffsetUtcConversionForNpgsql 可选补丁的单元测试。
/// </summary>
public class DateTimeOffsetUtcConversionTests
{
    [Fact]
    public void UseDateTimeOffsetUtcConversionForNpgsql_WhenEnabled_SetsValueConverterOnDateTimeOffsetProperties()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DateTimeOffsetEnabledTestDbContext>();
        optionsBuilder.UseInMemoryDatabase("DateTimeOffset_Enabled");
        optionsBuilder.UseDateTimeOffsetUtcConversionForNpgsql(enable: true);
        var options = optionsBuilder.Options;
        var extension = options.Extensions.OfType<NetCorePalDbContextOptionsExtension>().FirstOrDefault();
        Assert.NotNull(extension);
        Assert.True(extension.EnableDateTimeOffsetUtcConversion);

        var mediator = new ServiceCollection()
            .AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(DateTimeOffsetUtcConversionTests).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<IMediator>();

        using var context = new DateTimeOffsetEnabledTestDbContext(options, mediator);
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(EntityWithDateTimeOffset))!;
        var createdAtProp = entityType.FindProperty(nameof(EntityWithDateTimeOffset.CreatedAt))!;
        var optionalAtProp = entityType.FindProperty(nameof(EntityWithDateTimeOffset.OptionalAt))!;

        var converterCreated = createdAtProp.GetValueConverter();
        var converterOptional = optionalAtProp.GetValueConverter();

        Assert.NotNull(converterCreated);
        Assert.NotNull(converterOptional);

        // 写入方向应转为 UTC（非可空属性）
        var withOffset = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.FromHours(8));
        var converted = (DateTimeOffset)converterCreated!.ConvertToProvider(withOffset)!;
        Assert.Equal(TimeSpan.Zero, converted.Offset);
        Assert.Equal(withOffset.UtcDateTime, converted.UtcDateTime);

        // 可空属性：null 与 UTC 转换行为
        Assert.Null(converterOptional!.ConvertToProvider(null));
        var convertedValue = (DateTimeOffset?)converterOptional.ConvertToProvider((DateTimeOffset?)withOffset);
        Assert.NotNull(convertedValue);
        Assert.Equal(withOffset.UtcDateTime, convertedValue!.Value.UtcDateTime);
    }

    [Fact]
    public void UseDateTimeOffsetUtcConversionForNpgsql_WhenNotCalled_DoesNotSetUtcConverter()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DateTimeOffsetDisabledTestDbContext>();
        optionsBuilder.UseInMemoryDatabase("DateTimeOffset_Disabled");
        var mediator = new ServiceCollection()
            .AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(DateTimeOffsetUtcConversionTests).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<IMediator>();

        using var context = new DateTimeOffsetDisabledTestDbContext(optionsBuilder.Options, mediator);
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(EntityWithDateTimeOffset))!;
        var createdAtProp = entityType.FindProperty(nameof(EntityWithDateTimeOffset.CreatedAt))!;

        // 未启用扩展时，不应设置我们自定义的“写入转 UTC”的 Converter（内存库下通常无自定义 converter）
        var converter = createdAtProp.GetValueConverter();
        if (converter != null)
        {
            var withOffset = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.FromHours(8));
            var converted = (DateTimeOffset)converter.ConvertToProvider(withOffset)!;
            Assert.Equal(TimeSpan.FromHours(8), converted.Offset);
        }
    }

    [Fact]
    public void UseDateTimeOffsetUtcConversionForNpgsql_WhenDisabled_DoesNotSetUtcConverter()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DateTimeOffsetDisabledTestDbContext>();
        optionsBuilder.UseInMemoryDatabase("DateTimeOffset_ExplicitDisabled");
        optionsBuilder.UseDateTimeOffsetUtcConversionForNpgsql(enable: false);
        var mediator = new ServiceCollection()
            .AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(DateTimeOffsetUtcConversionTests).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<IMediator>();

        using var context = new DateTimeOffsetDisabledTestDbContext(optionsBuilder.Options, mediator);
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(EntityWithDateTimeOffset))!;
        var createdAtProp = entityType.FindProperty(nameof(EntityWithDateTimeOffset.CreatedAt))!;
        var converter = createdAtProp.GetValueConverter();

        if (converter != null)
        {
            var withOffset = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.FromHours(8));
            var converted = (DateTimeOffset)converter.ConvertToProvider(withOffset)!;
            Assert.Equal(TimeSpan.FromHours(8), converted.Offset);
        }
    }
}
