using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_DAS")]
public class Das
{
    [Key]
    [Column("DASID")]
    public int DasId { get; set; }

    [Column("DASREFERENCIA")]
    [MaxLength(50)]
    public string? DasReferencia { get; set; }

    [Column("DASDATAVENCIMENTO")]
    public DateTime? DasDataVencimento { get; set; }

    [Column("DASSTATUS")]
    public StatusDas DasStatus { get; set; } = StatusDas.Pendente;

    [Column("DASDTCADASTRO")]
    public DateTime? DasDtCadastro { get; set; }
}
