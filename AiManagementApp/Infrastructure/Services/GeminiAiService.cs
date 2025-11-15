
using AiManagementApp.Models;
using Google.GenAI;

namespace AiManagementApp.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly Client _client;

    // Atrela a API Key gerada ao modelo da AI, criando um novo cliente para ser utilizado
    public GeminiAiService(IConfiguration config)
    {
        var apiKey = config["GeminiAPIKey"]
            ?? throw new InvalidOperationException("GeminiAPIKey não configurada");
        
        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);

        _client = new Client();
    }
    
    // Cria o método para gerar a resposta da IA,
    // incluindo o prompt a ser solicitado e especificaçao de qual versao do Gemini vai ser utilizada
    public async Task<string> GerarRecomendacaoAsync(string resumo, AiLog.NivelRisco nivel)
    {
        var nivelDescricao = nivel switch
        {
            AiLog.NivelRisco.Leve      => "leve",
            AiLog.NivelRisco.Moderado  => "moderado",
            _                          => "desconhecido"
            };

        var prompt =
            $@"Usuário relatou: ""{resumo}""
            Nível emocional: {nivelDescricao}.

            Gere uma recomendação acolhedora e prática:
            - Validação emocional gentil
            - 2 a 4 sugestões simples
            - Para leve: respiração, pequenas pausas, algo prazeroso
            - Para moderado: grounding, pausa estruturada, falar com alguém de confiança
            - Nunca mencione IA ou termos médicos.
            - Escreva 4-6 frases.";

        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash",
            contents: prompt
            );

        var texto = response.Candidates.FirstOrDefault()?
            .Content.Parts.FirstOrDefault()?
            .Text ?? "";

        if (string.IsNullOrWhiteSpace(texto))
            texto = "Tente respirar fundo, fazer uma pausa curta e cuidar de si neste momento.";

        return texto.Trim();
    }
}