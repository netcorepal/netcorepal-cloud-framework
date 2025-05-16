using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class ReceivedMessageConfiguration : IEntityTypeConfiguration<ReceivedMessage>
{
    public void Configure(EntityTypeBuilder<ReceivedMessage> builder)
    {
        builder.ToTable(NetCorePalStorageOptions.ReceivedMessageTableName);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.Name).HasMaxLength(400).IsRequired();
        builder.Property(e => e.Group).HasMaxLength(200);
        builder.Property(e => e.Content).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Retries);
        builder.Property(e => e.Added).HasColumnType("datetime2(7)").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnType("datetime2(7)");
        builder.Property(e => e.StatusName).HasMaxLength(50).IsRequired();

        builder.HasIndex(e => new { e.Version, e.ExpiresAt, e.StatusName }, "IX_Version_ExpiresAt_StatusName");
        builder.HasIndex(e => new { e.ExpiresAt, e.StatusName }, "IX_ExpiresAt_StatusName");
    }
}