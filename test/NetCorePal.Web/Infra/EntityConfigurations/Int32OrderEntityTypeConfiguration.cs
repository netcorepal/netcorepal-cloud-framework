using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.EntityConfigurations
{
    internal class Int32OrderEntityTypeConfiguration : IEntityTypeConfiguration<Int32Order>
    {
        public void Configure(EntityTypeBuilder<Int32Order> builder)
        {
            builder.ToTable("int32order");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(b => b.Name).HasMaxLength(100);
        }
    }
}