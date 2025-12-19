using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class PublishedMessageConfiguration() : IEntityTypeConfiguration<PublishedMessage>
{
    public void Configure(EntityTypeBuilder<PublishedMessage> builder)
    {
        builder.ToCollection(NetCorePalStorageOptions.PublishedMessageTableName);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Content);
        builder.Property(e => e.Retries);
        builder.Property(e => e.Added).IsRequired();
        builder.Property(e => e.ExpiresAt);
        builder.Property(e => e.StatusName).HasMaxLength(40).IsRequired();
        if (NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled)
        {
            builder.Property(e => e.DataSourceName)
                .HasMaxLength(50);
        }
        else
        {
            builder.Ignore(e => e.DataSourceName);
        }
    }
}
