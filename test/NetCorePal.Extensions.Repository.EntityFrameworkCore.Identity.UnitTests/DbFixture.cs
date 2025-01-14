using Testcontainers.MySql;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity.UnitTests;

public class DbFixture : IAsyncLifetime
{
    public readonly MySqlContainer mySqlContainer = new MySqlBuilder()
        .WithUsername("root").WithPassword("123456")
        .WithEnvironment("TZ", "Asia/Shanghai")
        .WithDatabase("demo").Build();

    public async Task InitializeAsync()
    {
        await mySqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await mySqlContainer.DisposeAsync();
    }
}