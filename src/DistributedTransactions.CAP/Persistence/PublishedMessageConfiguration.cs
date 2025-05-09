using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class PublishedMessageConfiguration() : IEntityTypeConfiguration<PublishedMessage>
{
    public void Configure(EntityTypeBuilder<PublishedMessage> builder)
    {
        builder.ToTable("PublishedMessage");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Content);
        builder.Property(e => e.Retries);
        builder.Property(e => e.Added).IsRequired();
        builder.Property(e => e.ExpiresAt);
        builder.Property(e => e.StatusName).HasMaxLength(40).IsRequired();

        if (NetCorePalStorageOptions.Default!.EnableTenant)
        {
            builder.Property(e => e.TenantId)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue(string.Empty);
        }
        else
        {
            builder.Ignore(p => p.TenantId);
        }
        
        builder.HasIndex(e => new { e.Version, e.ExpiresAt, e.StatusName }, "IX_Version_ExpiresAt_StatusName");
        builder.HasIndex(e => new { e.ExpiresAt, e.StatusName }, "IX_ExpiresAt_StatusName");
    }
}