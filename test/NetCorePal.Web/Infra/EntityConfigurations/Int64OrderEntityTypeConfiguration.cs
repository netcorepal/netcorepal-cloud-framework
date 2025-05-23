using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.EntityConfigurations
{
    internal class Int64OrderEntityTypeConfiguration : IEntityTypeConfiguration<Int64Order>
    {
        public void Configure(EntityTypeBuilder<Int64Order> builder)
        {
            builder.ToTable("int64order");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseSnowFlakeValueGenerator();
            builder.Property(b => b.Name).HasMaxLength(100);
        }
    }
}