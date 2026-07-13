using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_ANO_ANOTACAO")]
public class Anotacao
{
    [Key]
    [Column("ANOID")]
    public int AnoId { get; set; }

    [Column("ANODESCRICAO")]
    [MaxLength(1000)]
    public string? AnoDescricao { get; set; }

    [Column("ANOOBSERVACOES")]
    [MaxLength(3000)]
    public string? AnoObservacoes { get; set; }

    [Column("ANOLINK")]
    [MaxLength(500)]
    public string? AnoLink { get; set; }

    /// <summary>ANOTACAO (padrão) ou REGRA_EMPRESA.</summary>
    [Column("ANOTIPO")]
    [MaxLength(30)]
    public string AnoTipo { get; set; } = "ANOTACAO";

    [Column("ANODTCADASTRO")]
    public DateTime? AnoDtCadastro { get; set; }
}
