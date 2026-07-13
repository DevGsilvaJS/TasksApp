using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_REG_REGIMENTO_ACEITE")]
public class RegimentoAceite
{
    [Key]
    [Column("RACID")]
    public int RacId { get; set; }

    [Column("REGID")]
    public int RegId { get; set; }

    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("RACACEITO")]
    public int RacAceito { get; set; }

    [Column("RACOBSERVACAO")]
    [MaxLength(2000)]
    public string? RacObservacao { get; set; }

    [Column("RACDATAACEITE")]
    public DateTime? RacDataAceite { get; set; }

    [ForeignKey(nameof(RegId))]
    public virtual Regimento Regimento { get; set; } = null!;

    [ForeignKey(nameof(UsuId))]
    public virtual Usuario Usuario { get; set; } = null!;
}
