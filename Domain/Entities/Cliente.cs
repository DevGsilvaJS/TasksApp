using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CLI_CLIENTE")]
public class Cliente
{
    [Key]
    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("PESID")]
    public int PesId { get; set; }

    [Column("CLICODIGO")]
    public int CliCodigo { get; set; }

    [Column("CLIDATACADASTRO")]
    public DateTime? CliDataCadastro { get; set; }

    [Column("CLIVALORCONTRATO", TypeName = "decimal(18,2)")]
    public decimal? CliValorContrato { get; set; }

    // Navegação
    [ForeignKey("PesId")]
    public virtual Pessoa Pessoa { get; set; } = null!;

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
