using Asp.Versioning.Builder;

namespace AiManagementApp.Endpoints;

public static class AiManagementAppEndpoints
{
    public static void RegisterAiManagementAppEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var v1Group = app.MapGroup("/api/v1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(1, 0)
            .WithTags("Lyra AI Endpoints V1");

        v1Group.MapAiLogEndpoints("V1");
        
        // Mantido para uma possível futura alteração
        var v2Group = app.MapGroup("/api/v2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(2, 0)
            .WithTags("Lyra AI Endpoints V2");
    }
}