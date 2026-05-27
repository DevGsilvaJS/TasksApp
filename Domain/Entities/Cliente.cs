using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_CLI_CLIENTE")]
public class Cliente
{
    [Key]
    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("CLICODIGO")]
    [MaxLength(20)]
    public string CliCodigo { get; set; } = string.Empty;

    [Column("PESID")]
    public int PesId { get; set; }

    [Column("CLIDATACADASTRO")]
    public DateTime? CliDataCadastro { get; set; }

    [Column("CLIVALORCONTRATO", TypeName = "decimal(18,2)")]
    public decimal? CliValorContrato { get; set; }

    [Column("CLIDATAFINALCONTRATO")]
    public DateTime? CliDataFinalContrato { get; set; }

    [Column("CLIDIAPAGAMENTO")]
    public int? CliDiaPagamento { get; set; }

    /// <summary>
    /// Dia do mês (1-31) em que vence a NF de serviço do cliente. Mensal.
    /// </summary>
    [Column("CLIDIANFSERVICO")]
    public int? CliDiaNfServico { get; set; }

    [Column("CLISTATUS")]
    public StatusCliente CliStatus { get; set; } = StatusCliente.Ativo;

    [Column("USUID")]
    public int UsuId { get; set; }

    // Navegação
    [ForeignKey("PesId")]
    public virtual Pessoa Pessoa { get; set; } = null!;

    [ForeignKey("UsuId")]
    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
    public virtual ICollection<AnotacaoCliente> AnotacoesCliente { get; set; } = new List<AnotacaoCliente>();
    public virtual ICollection<EnvioNotaServico> EnviosNotaServico { get; set; } = new List<EnvioNotaServico>();
    public virtual ICollection<ClienteContratoValor> ContratosValores { get; set; } = new List<ClienteContratoValor>();
}
