using NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<DateTime> UseDateTimeNowValueGenerator(
        this PropertyBuilder<DateTime> builder)
    {
        builder = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        return builder.HasValueGenerator<DateTimeNowValueGenerator>();
    }
}