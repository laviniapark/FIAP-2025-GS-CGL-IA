using System.ComponentModel.DataAnnotations.Schema;

namespace AiManagementApp.Models;

    public class AiLog
    {
        [Column("log_id")]
        public Guid Id { get; set; }

        [Column("data_hora_requisicao")]
        public DateTime DHRequisicao { get; set; }

        [Column("resumo_recebido", TypeName = "VARCHAR2(4000)")]
        public string ResumoRecebido { get; set; }

        [Column("recomendacao_gerada", TypeName = "VARCHAR2(4000)")]
        public string RecomendacaoGerada { get; set; }

        [Column("nivel")]
        public NivelRisco Nivel { get; set; }

        [Column("sucesso_envio", TypeName = "NUMBER(1)")]
        public bool SucessoEnvio { get; set; }

        public enum NivelRisco
        {
            Leve = 0,
            Moderado = 1
        }
    }