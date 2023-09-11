using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaEntityConfiguration : IEntityTypeConfiguration<SagaEntity>
    {
        public void Configure(EntityTypeBuilder<SagaEntity> builder)
        {
            builder.ToTable("sagadata");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedNever();
            builder.Property(t => t.SagaData).IsRequired();
        }
    }

}
