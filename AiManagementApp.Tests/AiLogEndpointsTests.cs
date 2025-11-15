using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiManagementApp.Tests;

[Collection("API Tests")]
public class AiLogEndpointsTests
{
    private readonly HttpClient _client;

    public AiLogEndpointsTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAiLogs_Retorna200EFormatoPaginadoComHateoas()
    {
        var response = await _client.GetAsync("/api/v1/ai-logs?PageNumber=1&PageSize=2");
        
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = json.RootElement;
        
        Assert.True(root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array,
            "A resposta não contem uma lista de logs");
        
        Assert.True(items.GetArrayLength() > 0, "A lista de logs retornada está vazia");
    }

    [Fact]
    public async Task PostAiLog_Retorna201ECriaNovoAiLog()
    {
        var novoLog = new
        {
            dhRequisicao = "2025-11-13T23:30:00",
            resumoRecebido = "Usuário relatou dificuldade para focar",
            recomendacaoGerada = "Resposta não retornada pela IA",
            nivel = 0,
            sucessoEnvio = false
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai-logs")
        {
            Content = JsonContent.Create(novoLog)
        };
        
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = json.RootElement;
        
        Assert.True(root.TryGetProperty("dhRequisicao", out var dhRequisicao));
        Assert.Equal("2025-11-13T23:30:00", dhRequisicao.GetString());
        
        Assert.True(root.TryGetProperty("nivel", out var nivel));
        Assert.Equal(0,  nivel.GetInt32());
    }

    [Fact]
    public async Task DeleteAiLog_Retorna200EMensagem()
    {
        var novoLog = new
        {
            dhRequisicao = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
            resumoRecebido = "Estou sentindo muita sonolencia durante o trabalho",
            recomendacaoGerada = "Recomendo que tente dormir mais cedo",
            nivel = 0,
            sucessoEnvio = true
        };

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai-logs")
        {
            Content = JsonContent.Create(novoLog)
        };
        
        var postResponse = await _client.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        
        using var postJson = await JsonDocument.ParseAsync(await postResponse.Content.ReadAsStreamAsync());
        var logId = postJson.RootElement.GetProperty("id").GetGuid();
        
        var deleteResponse = await _client.DeleteAsync($"/api/v1/ai-logs/{logId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        
        using var deleteJson = await JsonDocument.ParseAsync(await deleteResponse.Content.ReadAsStreamAsync());
        var message = deleteJson.RootElement.GetProperty("message").GetString();
        
        Assert.Equal("Registro deletado com sucesso!", message);
    }
}