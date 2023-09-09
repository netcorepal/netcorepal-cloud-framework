using FluentValidation.AspNetCore;
using FluentValidation;
using NetCorePal.Extensions.Domain.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Reflection;
using NetCorePal.Web.Infra;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverterFactory());
});
var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);


#region 公共服务

builder.Services.AddSingleton<IClock, SystemClock>();

#endregion


#region 集成事件

//builder.Services.AddTransient<OrderPaidIntegrationEventHandler>();

#endregion

#region 模型验证器

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

#endregion


#region Mapper Provider

builder.Services.AddMapperPrivider(Assembly.GetExecutingAssembly());

#endregion

#region 基础设施

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()).AddUnitOfWorkBehaviors());
builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //options.UseInMemoryDatabase("ApplicationDbContext");

    // options.UseMySql(builder.Configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")));
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
    options.LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});
builder.Services.AddUnitOfWork<ApplicationDbContext>();
builder.Services.AddPostgreSqlTransactionHandler();
builder.Services.AddAllCAPEventHanders(typeof(Program));
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<ApplicationDbContext>();
    x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
});
#endregion

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => "Hello World!");

app.Run();


public partial class Program
{
}