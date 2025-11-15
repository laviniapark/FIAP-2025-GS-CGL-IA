using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AiManagementApp.Infrastructure.Services;

public static class AiManagementAppServices
{
    public static void RegisterAiManagementAppServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        #region Database

        if (environment.IsEnvironment("Testing"))
        {
            services.AddDbContext<AiManagementAppDb>(options =>
                options.UseInMemoryDatabase("AiManagementAppDb"));
        }
        else
        {
            services.AddDbContext<AiManagementAppDb>(options =>
                options.UseOracle(configuration.GetConnectionString("DefaultConnection")));
        }

        #endregion

        #region JsonContent
        
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.WriteIndented = true;
        });

        #endregion
        
        #region Scalar/Swagger/OpenApi

        services.AddOpenApi();

        #endregion
        
        #region Versioning

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
        });

        #endregion

        #region OpenTelemetry

        if (!environment.IsEnvironment("Testing"))
        {
            Console.WriteLine(">> OpenTelemetry ATIVADO");

            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource("AiManagementApp")
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService("AiManagementApp")
                        ).AddOtlpExporter()
                        .AddConsoleExporter();
                })
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation();
                });
        }
        else
        {
            Console.WriteLine(">> [TEST] OpenTelemetry DESATIVADO");
        }

        #endregion
        
        #region HealthCheck
        
        services.AddHealthChecks()
            .AddOracle(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "oracle-db-aimanagement",
                healthQuery:"SELECT 1 FROM DUAL",
                failureStatus:HealthStatus.Degraded,
                timeout:TimeSpan.FromSeconds(10),
                tags:new[]{"oracle","database"}
            );
        
        services.AddHealthChecksUI().AddInMemoryStorage();
        
        #endregion

        #region AI Service

        services.AddScoped<IAiService, GeminiAiService>();

        #endregion
    }
}