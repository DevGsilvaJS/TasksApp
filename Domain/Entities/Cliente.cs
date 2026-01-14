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
    public int CliCodigo { get; set; }

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

    [Column("CLISTATUS")]
    public StatusCliente CliStatus { get; set; } = StatusCliente.Ativo;

    // Navegação
    [ForeignKey("PesId")]
    public virtual Pessoa Pessoa { get; set; } = null!;

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
