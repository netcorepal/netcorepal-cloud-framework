using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.EntityConfigurations
{
    internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("order");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd().UseSnowFlakeValueGenerator();
            builder.Property(b => b.Name).HasMaxLength(100);
            builder.Property(b => b.Count);
            builder.Property(b => b.Paid);
            builder.HasMany(b => b.OrderItems).WithOne().HasForeignKey(p => p.OrderId);
            builder.Navigation(b => b.OrderItems).AutoInclude();
            //builder.Property(b => b.RowVersion).IsNetCorePalRowVersion();
            builder.Property(b => b.CreateTime).ValueGeneratedOnAddOrUpdate().UseDateTimeNowValueGenerator();
        }
    }
}