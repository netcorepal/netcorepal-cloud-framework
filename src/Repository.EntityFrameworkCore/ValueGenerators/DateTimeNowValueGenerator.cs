using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

public class DateTimeNowValueGenerator: ValueGenerator<DateTime>
{
    public override DateTime Next(EntityEntry entry)
    {
        return DateTime.UtcNow;
    }

    public override bool GeneratesTemporaryValues => false;
}