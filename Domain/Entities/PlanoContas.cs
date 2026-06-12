using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_PLC_PLANO_CONTAS")]
public class PlanoContas
{
    [Key]
    [Column("PLCID")]
    public int PlcId { get; set; }

    [Column("PLCDESCRICAO")]
    [MaxLength(500)]
    public string PlcDescricao { get; set; } = string.Empty;
}
