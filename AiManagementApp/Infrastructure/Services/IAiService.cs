using AiManagementApp.Models;

namespace AiManagementApp.Infrastructure.Services;

public interface IAiService
{
    /// <summary>
    /// Gera uma recomendação de bem-estar com base no resumo do usuário e no nível de risco.
    /// </summary>
    /// <param name="resumo">Resumo enviado pelo usuário (vindo do Java).</param>
    /// <param name="nivel">Nível de risco (leve ou moderado).</param>
    /// <returns>Texto de recomendação gerado pela IA.</returns>
    Task<string> GerarRecomendacaoAsync(string resumo, AiLog.NivelRisco nivel);
}