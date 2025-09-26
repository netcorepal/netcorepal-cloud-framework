using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class PublishedMessageConfiguration() : IEntityTypeConfiguration<PublishedMessage>
{
    public static bool PublishedMessageShardingDatabaseEnabled { get; set; } = false;

    public void Configure(EntityTypeBuilder<PublishedMessage> builder)
    {
        builder.ToTable(NetCorePalStorageOptions.PublishedMessageTableName);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Content).HasColumnType("TEXT");
        builder.Property(e => e.Retries);
        builder.Property(e => e.Added).HasColumnType("TIMESTAMP").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnType("TIMESTAMP");
        builder.Property(e => e.StatusName).HasMaxLength(40).IsRequired();
        if (PublishedMessageShardingDatabaseEnabled)
        {
            builder.Property(e => e.DataSourceName)
                .HasMaxLength(50);
        }
        else
        {
            builder.Ignore(e => e.DataSourceName);
        }

        builder.HasIndex(e => new { e.Version, e.ExpiresAt, e.StatusName }, "IX_Version_ExpiresAt_StatusName");
        builder.HasIndex(e => new { e.ExpiresAt, e.StatusName }, "IX_ExpiresAt_StatusName");
    }
}