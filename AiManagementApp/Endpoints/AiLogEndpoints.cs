using System.ComponentModel;
using AiManagementApp.Infrastructure;
using AiManagementApp.Infrastructure.Hateoas;
using AiManagementApp.Infrastructure.Pagination;
using AiManagementApp.Models;
using AiManagementApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AiManagementApp.Endpoints;

public static class AiLogEndpoints
{
    public static RouteGroupBuilder MapAiLogEndpoints(this RouteGroupBuilder builder, string version)
    {
        var group = builder.MapGroup("ai-logs")
            .WithTags("AI Log Endpoints");

        // GET ALL
        group.MapGet("", async ([AsParameters] PageParameters pageParam, AiManagementAppDb db, LinkGenerator lg, HttpContext http) =>
        {
            var page = pageParam.PageNumber < 1 ? 1 : pageParam.PageNumber;
            var size =  pageParam.PageSize is < 1 or > 100 ? 20 :  pageParam.PageSize;

            var query = db.AiLogs
                .AsNoTracking()
                .OrderByDescending(l => l.DHRequisicao)
                .Select(l => new AiLogResponse(
                    l.Id,
                    l.DHRequisicao,
                    l.ResumoRecebido,
                    l.RecomendacaoGerada,
                    l.Nivel,
                    l.SucessoEnvio
                    ));

            var paged = await PagedList<AiLogResponse>.CreateAsync(query, page, size);

            var itemWLinks = paged.Items
                .Select(item => new AiLogResponseHO(
                    item.Id,
                    item.DHRequisicao,
                    item.ResumoRecebido,
                    item.RecomendacaoGerada,
                    item.Nivel,
                    item.SucessoEnvio,
                    AiLogLinkHelper.CreateItemLinks(item.Id, lg, http)
                    )).ToList();

            var response = new PagedHateoasResponse<AiLogResponseHO>
            {
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                HasNext = paged.HasNextPage,
                HasPrevious = paged.HasPreviousPage,
                Links =
                    PaginationLinkHelper.CreatePageLinks(lg, http, page, size, paged.HasNextPage,
                        paged.HasPreviousPage),
                Items = itemWLinks
            };
            
            return Results.Ok(response);
        }).WithName("GetLogsPaginatedList")
            .WithSummary("Lista paginada com HATEOAS")
            .WithDescription(
                "Retorna a lista paginada de logs ordenada da data mais recente para a mais antiga, juntamente com hiperlinks para navegação. " +
                "Permite definir o tamanho da página. Inclui os registros, número da página atual, " +
                "quantidade de itens por página, total de registros, e indicadores de próxima e anterior página.")
            .Produces<PagedHateoasResponse<AiLogResponseHO>>(StatusCodes.Status200OK);
        
        // GET BY ID
        group.MapGet("/{id:guid}",
            async
                (AiManagementAppDb db, [Description("Identificador unico do log")] Guid id, LinkGenerator lg,
                    HttpContext http) =>
                {
                    var log = await db.AiLogs
                        .AsNoTracking()
                        .SingleOrDefaultAsync(l => l.Id == id);

                    if (log is null)
                    {
                        return Results.NotFound(new { message = "Registro de log não encontrado", id });   
                    }
                    
                    List<LinkDTO> links = new()
                    {
                        new(lg.GetPathByName(http, "GetLogById", new { id })!, "self", "GET"),
                        new(lg.GetPathByName(http, "UpdateLog", new { id })!, "update", "PUT"),
                        new(lg.GetPathByName(http, "DeleteLog", new { id })!, "delete", "DELETE")
                    };
                    
                    var response = new AiLogResponseHO(
                        log.Id,
                        log.DHRequisicao,
                        log.ResumoRecebido,
                        log.RecomendacaoGerada,
                        log.Nivel,
                        log.SucessoEnvio,
                        links
                        );

                    return Results.Ok(response);
                }).WithName("GetLogById")
            .WithSummary("Retorno de um registro do log específico")
            .WithDescription("Retorna um registro de log buscando pelo ID. " +
                             "Caso o ID passado esteja incorreto ou nao existe, retorna o erro 404.")
            .Produces<AiLogResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // POST
        group.MapPost("", async (AiManagementAppDb db, AiLogRequest req) =>
        {
            if (req.DHRequisicao > DateTime.UtcNow.AddMinutes(5))
            {
                return Results.BadRequest(new { message = "DHRequisicao não pode ser no futuro." });
            }

            if (string.IsNullOrWhiteSpace(req.ResumoRecebido))
            {
                return Results.BadRequest(new { message = "ResumoRecebido é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(req.RecomendacaoGerada))
            {
                return Results.BadRequest(new { message = "RecomendacaoGerada é obrigatória." });   
            }
            
            if (!Enum.IsDefined(typeof(AiLog.NivelRisco), req.Nivel))
            {
                return Results.BadRequest(new { message = "Nível inválido. Insira 0 para Leve ou 1 para Moderado." });
            }
            
            var log = new AiLog
            {
                Id = Guid.NewGuid(),
                DHRequisicao = req.DHRequisicao,
                ResumoRecebido = req.ResumoRecebido,
                RecomendacaoGerada = req.RecomendacaoGerada,
                Nivel = req.Nivel,
                SucessoEnvio = req.SucessoEnvio
            };

            db.AiLogs.Add(log);
            await db.SaveChangesAsync();

            var response = new AiLogResponse(
                log.Id,
                log.DHRequisicao,
                log.ResumoRecebido,
                log.RecomendacaoGerada,
                log.Nivel,
                log.SucessoEnvio
                );
            
            return Results.Created($"/ai-logs/{log.Id}",response);
        }).WithSummary("Cadastro de novo registro de log")
            .WithDescription("Cadastra um novo registro de log no sistema. " +
                             "Assim que o cadastro for efetuado, retorna código 201. " +
                             "Caso nao passe pela validações, irá retornar 400 explicando o motivo. " +
                             "Esse método é puramente para fins didáticos, pois esses dados serão gerados automaticamente pela IA.")
            .Produces<AiLogResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
        
        // PUT
        group.MapPut("/{id:guid}",
            async
                ([Description("Identificador único do registro")] Guid id, AiLogRequest req, AiManagementAppDb db,
                    LinkGenerator lg, HttpContext http) =>
                {
                    var log = await db.AiLogs.FirstOrDefaultAsync(l => l.Id == id);

                    if (log is null)
                    {
                        return Results.NotFound(new { message = "Registro não encontrado", id });
                    }

                    var links = new List<LinkDTO>
                    {
                        new(lg.GetPathByName(http, "GetLogById", new { id })!, "getById", "GET"),
                        new(lg.GetPathByName(http, "UpdateLog", new { id })!, "self", "PUT"),
                        new(lg.GetPathByName(http, "DeleteLog", new { id })!, "delete", "DELETE")
                    };

                    log.DHRequisicao = req.DHRequisicao;
                    log.ResumoRecebido = req.ResumoRecebido;
                    log.RecomendacaoGerada = req.RecomendacaoGerada;
                    log.Nivel = req.Nivel;
                    log.SucessoEnvio = req.SucessoEnvio;

                    await db.SaveChangesAsync();

                    var response = new
                    {
                        message = "Registro atualizado com sucesso!",
                        data = new AiLogResponseHO(
                            log.Id,
                            log.DHRequisicao,
                            log.ResumoRecebido,
                            log.RecomendacaoGerada,
                            log.Nivel,
                            log.SucessoEnvio,
                            links
                            )
                    };

                    return Results.Ok(response);
                }).WithName("UpdateLog")
            .WithSummary("Atualização de registro de log")
            .WithDescription("Atualiza um registro de log existente. " +
                             "Caso o ID passado esteja incorreto ou nao existe, retorna o erro 404. " +
                             "Esse método é puramente para fins didáticos, pois em um contexto real não há a necessidade de atualizar os dados.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // DELETE
        group.MapDelete("/{id:guid}",
            async ([Description("Identificador único do registro")] Guid id, AiManagementAppDb db) =>
            {
                
                var log = await db.AiLogs.FindAsync(id);

                if (log is null)
                {
                    return Results.NotFound(new { message = "Registro não encontrado", id});   
                }
                
                db.AiLogs.Remove(log);
                await db.SaveChangesAsync();
                
                return Results.Ok(new { message = "Registro deletado com sucesso!"});
            }).WithName("DeleteLog")
            .WithSummary("Remoção de um registro")
            .WithDescription("Exclui um registro existente especificado pelo seu ID. " +
                             "Caso tenha sucesso, ele irá retornar o código 200 juntamente com uma mensagem de sucesso. " +
                             "Caso o ID esteja incorreto ou nao existe, retorna o erro 404.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        return builder;
    }
}