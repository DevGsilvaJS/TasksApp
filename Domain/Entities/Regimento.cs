using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_REG_REGIMENTO")]
public class Regimento
{
    [Key]
    [Column("REGID")]
    public int RegId { get; set; }

    [Column("REGSTATUS")]
    public int RegStatus { get; set; } = (int)StatusRegimento.Ativo;

    [Column("REGTITULO")]
    [MaxLength(300)]
    public string RegTitulo { get; set; } = string.Empty;

    [Column("REGDESCRICAO")]
    public string RegDescricao { get; set; } = string.Empty;

    public ICollection<RegimentoAceite> Aceites { get; set; } = new List<RegimentoAceite>();
    public ICollection<RegimentoAceiteLog> LogsAceite { get; set; } = new List<RegimentoAceiteLog>();
}
