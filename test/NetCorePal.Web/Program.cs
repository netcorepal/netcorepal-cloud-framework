using System.Net;
using FluentValidation.AspNetCore;
using FluentValidation;
using NetCorePal.Extensions.Domain.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using NetCorePal.Web.Application.Queries;
using NetCorePal.OpenTelemetry.Diagnostics;
using NetCorePal.SkyApm.Diagnostics;
using NetCorePal.Web.HostedServices;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Diagnostics.CAP;

var cfg = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
        true)
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(cfg)
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Fatal)
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();
    // builder.Host.UseSerilog((context, services, configuration) => configuration
    //         .ReadFrom.Configuration(context.Configuration)
    //         .Enrich.FromLogContext()
    //         .WriteTo.Console()
    //         .MinimumLevel.Information(),
    //     writeToProviders: true);
    builder.Services.AddSkyAPM(ext => ext.AddAspNetCoreHosting()
        .AddCap()
        .AddNetCorePal(options =>
        {
            options.WriteCommandData = true;
            options.WriteDomainEventData = true;
            options.WriteIntegrationEventData = true;
            options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverterFactory());
        }));

    #region OpenTelemetry

    var otlpHttpUrl = builder.Configuration["OpenTelemetry:OtlpHttp"];
    var otel = builder.Services.AddOpenTelemetry();
    otel.ConfigureResource(resource =>
    {
        resource.AddTelemetrySdk();
        resource.AddEnvironmentVariableDetector();
        resource.AddService(builder.Configuration["ApplicationName"]!);
    });

    otel.WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddCapInstrumentation();
        tracing.AddSource("MySqlConnector");
        tracing.AddNetCorePalInstrumentation();
        tracing.SetSampler(new AlwaysOnSampler());
        //tracing.AddConsoleExporter();
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri($"{otlpHttpUrl}/v1/traces");
            otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            //otlpOptions.Headers = "Authorization=Basic YWRtaW46dGVzdEAxMjM=";
        });
    });

    //otel.WithMetrics(metrics =>
    //{
    //    metrics.AddAspNetCoreInstrumentation();
    //    metrics.AddRuntimeInstrumentation();
    //    metrics.AddMeter("MySqlConnector");
    //    //暂时不采集这个指标,因为会产生大量请求/v1/metrics日志.https://github.com/open-telemetry/opentelemetry-dotnet/issues/5717
    //    //metrics.AddHttpClientInstrumentation();
    //    //metrics.AddConsoleExporter();
    //    metrics.AddOtlpExporter(otlpOptions =>
    //    {
    //        otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
    //        otlpOptions.Endpoint = new Uri($"{otlpHttpUrl}/v1/metrics");
    //        //otlpOptions.Headers = $"Authorization=132435";
    //    });
    //});

    #endregion

    builder.Services.AddHealthChecks();

    builder.Services.AddMvc()
        .AddControllersAsServices()
        .AddEntityIdSystemTextJson()
        .AddKnownExceptionModelBinderErrorHandler();
    var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);

    #region 公共服务

    builder.Services.AddSingleton<IClock, SystemClock>();
    builder.Services.AddNetCorePalServiceDiscoveryClient();

    #endregion


    #region 集成事件

    //builder.Services.AddTransient<OrderPaidIntegrationEventHandler>();

    #endregion

    #region 模型验证器

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddKnownExceptionErrorModelInterceptor();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    #endregion


    #region Mapper Provider

    builder.Services.AddMapperPrivider(Assembly.GetExecutingAssembly());

    #endregion

    builder.Services.AddScoped<OrderQuery>();

    #region 基础设施

    builder.Services.AddContext().AddEnvContext().AddCapContextProcessor();
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()
            .AddUnitOfWorkBehaviors()
            .AddKnownExceptionValidationBehavior());
    builder.Services.AddCommandLocks(typeof(Program).Assembly);
    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
#if NET10_0
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
#else
        options.UseMySql(builder.Configuration.GetConnectionString("Mysql"),
            new MySqlServerVersion(new Version(8, 0, 34)),
            b => { b.MigrationsAssembly(typeof(Program).Assembly.FullName); });
#endif
        var f = new LoggerFactory();
        f.AddSerilog();
        options.UseLoggerFactory(f)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });

    builder.Services.AddUnitOfWork<ApplicationDbContext>();
#if NET10_0
    builder.Services.AddPostgreSqlTransactionHandler();
#else
    //builder.Services.AddPostgreSqlTransactionHandler();
    builder.Services.AddMySqlTransactionHandler();
#endif
    builder.Services.AddIntegrationEventServices(typeof(Program))
        .UseCap(typeof(Program))
        .AddIIntegrationEventConverter(typeof(Program))
        .AddContextIntegrationFilters()
        .AddTransactionIntegrationEventHandlerFilter();
    builder.Services.AddCap(x =>
    {
        x.UseEntityFramework<ApplicationDbContext>();
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
        x.UseDashboard();
    });
    builder.Services.AddSagas<ApplicationDbContext>(typeof(Program)).AddCAPSagaEventPublisher();
    builder.Services.AddMultiEnv(builder.Configuration.GetSection("Env"))
        .AddEnvIntegrationFilters().AddEnvServiceSelector();
#if NET8_0
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = $"netcorepal-web"
        });
        var xmlFilename = $"{typeof(Program).Assembly.GetName().Name}.xml";
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });
#endif

    #endregion

    builder.Services.AddHostedService<CreateOrderCommandBackgroundService>();
    builder.Services.AddRedisLocks();
    var app = builder.Build();
    app.UseContext();
    //app.UseKnownExceptionHandler();
    app.UseKnownExceptionHandler(context =>
        {
            if (context.Request.Path.StartsWithSegments("/service"))
            {
                return new KnownExceptionHandleMiddlewareOptions()
                {
                    KnownExceptionStatusCode = HttpStatusCode.BadRequest,
                    UnknownExceptionStatusCode = HttpStatusCode.BadGateway
                };
            }

            return new KnownExceptionHandleMiddlewareOptions();
        }
    );
#if NET8_0
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Web AppV1"); });
#endif
    app.UseRouting();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapGet("/", () => "Hello World!");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// 
/// </summary>
public partial class Program
{
    /// <summary>
    /// 
    /// </summary>
    protected Program()
    {
    }
}