using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ServiceDiscovery;
using NetCorePal.Context;
#if NET8_0
using Microsoft.OpenApi.Models;
#endif
using NetCorePal.Web.Application.Queries;
using NetCorePal.OpenTelemetry.Diagnostics;
using NetCorePal.SkyApm.Diagnostics;
using NetCorePal.Web;
using NetCorePal.Web.Clients;
using FluentValidation;
using FluentValidation.AspNetCore;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.Domain.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.HostedServices;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Diagnostics.CAP;
using StackExchange.Redis;

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
    builder.Services.AddDataProtection();
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
            options.JsonSerializerOptions.AddNetCorePalJsonConverters();
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

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddNetCorePalJwt().AddRedisStore();
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(configureOptions =>
        {
            configureOptions.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            };
        }).AddJwtBearer(options =>
        {
            options.Authority = "netcorepal";
            options.Audience = "netcorepal";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateIssuer = false;
        });

    builder.Services.AddMvc()
        .AddControllersAsServices()
        .AddNetCorePalSystemTextJson()
        .AddKnownExceptionModelBinderErrorHandler();
    var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);

    #region 公共服务

    builder.Services.AddSingleton<IClock, SystemClock>();
    builder.Services.AddNetCorePalServiceDiscoveryClient();


    builder.Services.AddServiceDiscovery().AddConfigurationServiceEndpointProvider();
    builder.Services.ConfigureHttpClientDefaults(d => d.AddServiceDiscovery());


    RefitSettings refitSettings = new RefitSettings();
    var jsonOptions = new JsonSerializerOptions();
    jsonOptions.AddNetCorePalJsonConverters();
    var serializer = new SystemTextJsonContentSerializer(jsonOptions);
    builder.Services.AddRefitClient<ICatalogApi>(_ => refitSettings)
        .ConfigureHttpClient(p => p.BaseAddress = new Uri("https://catalog:5000"))
        .AddServiceDiscovery()
        .AddStandardResilienceHandler();

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
            .AddKnownExceptionValidationBehavior()
            .AddUnitOfWorkBehaviors());
    builder.Services.AddCommandLocks(typeof(Program).Assembly);
    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseLazyLoadingProxies();
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

    builder.Services.AddIntegrationEvents(typeof(Program))
        .UseCap(b =>
        {
            b.RegisterServicesFromAssemblies(typeof(Program));
            b.AddContextIntegrationFilters();
#if NET10_0
            b.UsePostgreSql();
#else
            b.UseMySql();
#endif
        });
    builder.Services.AddCap(x =>
    {
        x.UseEntityFramework<ApplicationDbContext>();
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
        x.UseDashboard();
    });
    builder.Services.AddMultiEnv(builder.Configuration.GetSection("Env"))
        .UseNetCorePalServiceDiscovery();

    builder.Services.AddMultiEnv(options =>
        {
            options.ServiceName = "MyServiceName";
            options.ServiceEnv = "main";
        })
        .UseNetCorePalServiceDiscovery();
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
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, TestAuthorizationPolicyProvider>();
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
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapGet("/", () => "Hello World!");

    app.Use((context, next) =>
    {
        var contextAccessor = context.RequestServices.GetRequiredService<IContextAccessor>();
        contextAccessor.SetContext(new EnvContext("v2"));
        return next();
    });
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