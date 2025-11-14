using AiManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiManagementApp.Infrastructure.Config;

public class AiLogConfig : IEntityTypeConfiguration<AiLog>
{
    public void Configure(EntityTypeBuilder<AiLog> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.DHRequisicao).IsRequired().HasColumnType("DATE");
        builder.Property(i => i.ResumoRecebido).IsRequired();
        builder.Property(i => i.RecomendacaoGerada).IsRequired();
        builder.Property(i => i.Nivel).IsRequired().HasConversion<int>();
        builder.Property(i => i.SucessoEnvio).IsRequired()
            .HasConversion(
                v => v ? "Y" : "N",
                v => v == "Y"
            )
            .HasColumnType("CHAR(1)");
        
        builder.HasData(
            new AiLog
            {
                Id = Guid.Parse("a3bdc1f2-8c32-4a1a-9e71-8f2f3d16a101"),
                DHRequisicao = new DateTime(2025, 11, 07, 10, 15, 0),
                ResumoRecebido = "Usuário relatou excesso de carga emocional no trabalho.",
                RecomendacaoGerada = "Percebo que você está carregando muita coisa ao mesmo tempo. Experimente fazer uma pausa curta, respirar fundo por alguns instantes e organizar suas prioridades com calma. Pequenos intervalos podem aliviar bastante a pressão.",
                Nivel = AiLog.NivelRisco.Moderado,
                SucessoEnvio = true
            },
            new AiLog
            {
                Id = Guid.Parse("f41e7692-57d7-4384-9bc8-1165c6d3f0e2"),
                DHRequisicao = new DateTime(2025, 11, 08, 10, 32, 0),
                ResumoRecebido = "Usuário descreveu dificuldade para dormir.",
                RecomendacaoGerada = "Seu corpo pode estar pedindo um descanso mais estruturado. Tente evitar telas alguns minutos antes de dormir e procure desacelerar o ritmo gradualmente. Pequenos ajustes na rotina noturna podem ajudar bastante.",
                Nivel = AiLog.NivelRisco.Leve,
                SucessoEnvio = true
            },
            new AiLog
            {
                Id = Guid.Parse("3e1acf1d-7827-4c4c-b7c7-bc1bb909a2e9"),
                DHRequisicao = new DateTime(2025, 11, 09, 11, 23, 0),
                ResumoRecebido = "Usuário relatou irritabilidade com colegas.",
                RecomendacaoGerada = "É totalmente compreensível se sentir irritado em dias mais tensos. Se puder, tire um momento para respirar profundamente e se afastar um pouco da situação. Às vezes, alguns segundos de calma já ajudam a clarear a cabeça.",
                Nivel = AiLog.NivelRisco.Moderado,
                SucessoEnvio = true
            },
            new AiLog
            {
                Id = Guid.Parse("2c5e62e7-0265-4a12-a184-55c5c8eb44c7"),
                DHRequisicao = new DateTime(2025, 11, 10, 11, 46, 0),
                ResumoRecebido = "Usuário relatou sensação de desmotivação pela manhã.",
                RecomendacaoGerada = "É normal ter dias em que a motivação demora a aparecer. Que tal começar com uma tarefa bem simples? Cumprir um pequeno passo pode ajudar a criar um ritmo melhor para o restante do dia.",
                Nivel = AiLog.NivelRisco.Leve,
                SucessoEnvio = true
            },
            new AiLog
            {
                Id = Guid.Parse("d2c17420-9f84-4ee4-8c20-102875f79d0b"),
                DHRequisicao = new DateTime(2025, 11, 11, 12, 15, 0),
                ResumoRecebido = "Usuário relatou tensão acumulada após uma reunião difícil.",
                RecomendacaoGerada = "Reuniões intensas podem realmente deixar o corpo e a mente sobrecarregados. Tente alongar os ombros e o pescoço por alguns segundos e respirar lentamente. Isso pode ajudar a liberar parte dessa tensão.",
                Nivel = AiLog.NivelRisco.Moderado,
                SucessoEnvio = true
            },
            new AiLog
            {
                Id = Guid.Parse("e4a7f0fa-3e44-4fd1-a92d-cdc7284a88bb"),
                DHRequisicao = new DateTime(2025, 11, 12, 12, 30, 0),
                ResumoRecebido = "Usuário relatou desmotivação leve após manhã improdutiva.",
                RecomendacaoGerada = "Tudo bem ter uma manhã mais devagar. Você pode tentar dividir suas próximas tarefas em passos menores — isso ajuda a reconstruir o ritmo sem se sentir pressionado. Vá no seu tempo.",
                Nivel = AiLog.NivelRisco.Leve,
                SucessoEnvio = true
            }
        );
    }
}