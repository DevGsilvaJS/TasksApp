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

    [Column("ANOLINK")]
    [MaxLength(500)]
    public string? AnoLink { get; set; }

    [Column("ANODTCADASTRO")]
    public DateTime? AnoDtCadastro { get; set; }
}
