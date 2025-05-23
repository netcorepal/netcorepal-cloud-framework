using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.EntityConfigurations
{
    internal class GuidOrderEntityTypeConfiguration : IEntityTypeConfiguration<GuidOrder>
    {
        public void Configure(EntityTypeBuilder<GuidOrder> builder)
        {
            builder.ToTable("guidorder");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseGuidVersion7ValueGenerator();
            builder.Property(b => b.Name).HasMaxLength(100);
        }
    }
}