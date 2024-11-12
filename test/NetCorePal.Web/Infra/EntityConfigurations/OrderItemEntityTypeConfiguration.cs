using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NetCorePal.Web.Infra.EntityConfigurations;

/// <summary>
/// 
/// </summary>
public class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("orderitem");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator();
        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.Count);
        //builder.Property(b => b.RowVersion).IsNetCorePalRowVersion();
    }
}