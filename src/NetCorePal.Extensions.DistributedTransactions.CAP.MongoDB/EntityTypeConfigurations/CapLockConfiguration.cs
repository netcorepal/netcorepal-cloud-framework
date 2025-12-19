using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class CapLockConfiguration : IEntityTypeConfiguration<CapLock>
{
    public void Configure(EntityTypeBuilder<CapLock> builder)
    {
        builder.ToCollection(NetCorePalStorageOptions.LockTableName);
        builder.HasKey(e => e.Key);
        builder.Property(e => e.Key).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Instance).HasMaxLength(256);
        builder.Property(e => e.LastLockTime);
    }
}
