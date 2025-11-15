using AiManagementApp.Infrastructure;
using AiManagementApp.Infrastructure.Services;
using AiManagementApp.Models;
using AiManagementApp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AiManagementApp.Endpoints ;

    public static class GeminiAiEndpoints
    {
        public static RouteGroupBuilder MapGeminiAiEndpoints(this RouteGroupBuilder builder, string version)
        {
            var group = builder.MapGroup("ai")
                .WithTags("Gemini AI Endpoints");
            
            // Metodo POST que recebe os dados de Java e gera a recomencadacao da IA
            // Depois registra o historico no banco, independente de sucesso ou erro
            group.MapPost("/solicitar", async (IAiService ai, AiManagementAppDb db, [FromBody] AiLogRequestJava req, ILogger<AiLog> logger) =>
            {
                logger.LogInformation("Requisição recebida! Resumo: {ResumoRecebido} | Nível: {Nivel}", req.ResumoRecebido, req.Nivel);

                string textoGerado = "";
                bool sucesso = false;

                try
                {
                    textoGerado = await ai.GerarRecomendacaoAsync(req.ResumoRecebido, req.Nivel);
                    sucesso = true;
                }
                catch (Exception ex)
                {
                    logger.LogError("Erro ao gerar recomendação da IA");
                    textoGerado =
                        "Não consegui gerar uma recomendação agora, mas tente respirar fundo e fazer uma pausa curta.";
                }

                var log = new AiLog
                {
                    Id = Guid.NewGuid(),
                    DHRequisicao = DateTime.Now,
                    ResumoRecebido = req.ResumoRecebido,
                    RecomendacaoGerada = textoGerado,
                    Nivel = req.Nivel,
                    SucessoEnvio = sucesso
                };
                
                db.AiLogs.Add(log);
                await db.SaveChangesAsync();
                
                logger.LogInformation("Log salvo com sucesso! ID: {Id}", log.Id);
                
                return Results.Ok(new { recomendacao = textoGerado, id = log.Id });
            }).WithName("PostAi")
                .WithSummary("Recebe os dados de Java e envia pra IA")
                .WithDescription("Esse método irá receber os dados de Java, e irão enviá-los para a IA gerar a resposta. " +
                                 "Independente se dê certo ou não, ele irá salvar os dados no banco, indicando em Sucesso se deu certo ou não")
                .Produces(StatusCodes.Status200OK);

            // Método GET para teste local da integracao com a IA usando dados mockados,
            // também salvando o resultado no banco para manter histórico
            group.MapGet("/teste", async (IAiService ai, AiManagementAppDb db, ILogger<AiLog> logger) =>
            {
                string textoGerado = "";
                bool sucesso = false;

                try
                {
                    textoGerado = await ai.GerarRecomendacaoAsync(
                        "Estou meio cansada hoje",
                        AiLog.NivelRisco.Leve
                        );

                    sucesso = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao testar IA");

                    textoGerado =
                        "Não consegui testar a recomendação agora, mas tente respirar fundo e fazer uma pausa curta.";
                }

                var log = new AiLog
                {
                    Id = Guid.NewGuid(),
                    DHRequisicao = DateTime.Now,
                    ResumoRecebido = "Teste automático da IA",
                    RecomendacaoGerada = textoGerado,
                    Nivel = AiLog.NivelRisco.Leve,
                    SucessoEnvio = sucesso
                };

                db.AiLogs.Add(log);
                await db.SaveChangesAsync();

                logger.LogInformation("Log do teste salvo com sucesso no banco! ID = {Id}", log.Id);
                
                return Results.Ok(new { recomendacao = textoGerado, sucesso });
            })
                .WithName("GetAi")
                .WithSummary("Método para teste local da IA")
                .WithDescription("Utiliza dados mockados para testar a responsividade da IA e registra um log de teste no banco")
                .Produces(StatusCodes.Status200OK);



            return group;
        }
    }