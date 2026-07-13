using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_REG_REGIMENTO_ACEITE_LOG")]
public class RegimentoAceiteLog
{
    [Key]
    [Column("RALID")]
    public int RalId { get; set; }

    [Column("REGID")]
    public int RegId { get; set; }

    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("RALTIPO")]
    public int RalTipo { get; set; }

    [Column("RALDECISAO")]
    public int? RalDecisao { get; set; }

    [Column("RALOBSERVACAO")]
    [MaxLength(2000)]
    public string? RalObservacao { get; set; }

    [Column("RALDATA")]
    public DateTime RalData { get; set; }

    [ForeignKey(nameof(RegId))]
    public virtual Regimento Regimento { get; set; } = null!;

    [ForeignKey(nameof(UsuId))]
    public virtual Usuario Usuario { get; set; } = null!;
}
