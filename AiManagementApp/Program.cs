using AiManagementApp.Endpoints;
using AiManagementApp.Infrastructure.Services;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterAiManagementAppServices(
    builder.Configuration,
    builder.Environment);

builder.Services.AddOpenApi();

var app = builder.Build();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersions(new List<ApiVersion> {
        new ApiVersion(1, 0),
        new ApiVersion(2, 0)
    })
    .Build();

app.RegisterAiManagementAppEndpoints(apiVersionSet);

app.MapGet("/", () => "Insira na url '/health-ui' para visualizar a saude do Banco")
    .WithName("Introdução")
    .WithApiVersionSet(apiVersionSet)
    .MapToApiVersion(1,0);

app.MapHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
})
    .WithName("Health Check")
    .WithTags("Health Check Endpoint");

app.UseStaticFiles();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();