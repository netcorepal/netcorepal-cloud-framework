using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class ReceivedMessageConfiguration : IEntityTypeConfiguration<ReceivedMessage>
{
    public void Configure(EntityTypeBuilder<ReceivedMessage> builder)
    {
        builder.ToCollection(NetCorePalStorageOptions.ReceivedMessageTableName);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.Name).HasMaxLength(400).IsRequired();
        builder.Property(e => e.Group).HasMaxLength(200);
        builder.Property(e => e.Content);
        builder.Property(e => e.Retries);
        builder.Property(e => e.Added).IsRequired();
        builder.Property(e => e.ExpiresAt);
        builder.Property(e => e.StatusName).HasMaxLength(50).IsRequired();
    }
}
