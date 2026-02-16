using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_ANC_ANOTACAO_CLIENTE")]
public class AnotacaoCliente
{
    [Key]
    [Column("ANCID")]
    public int AncId { get; set; }

    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("ANCDESCRICAO")]
    [MaxLength(3000)]
    public string? AncDescricao { get; set; }

    [Column("ANCDTCADASTRO")]
    public DateTime? AncDtCadastro { get; set; }

    [ForeignKey("CliId")]
    public virtual Cliente Cliente { get; set; } = null!;
}
