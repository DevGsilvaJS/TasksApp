using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_DUP_DUPLICATA")]
public class Duplicata
{
    [Key]
    [Column("DUPID")]
    public int DupId { get; set; }

    [Column("DUPNUMERO")]
    public int DupNumero { get; set; }

    [Column("DUPDATAEMISSAO")]
    public DateTime DupDataEmissao { get; set; }

    [Column("DUPNUMEROPARCELAS")]
    public int DupNumeroParcelas { get; set; }

    [Column("DUPDESCRICAODESPESA")]
    [MaxLength(500)]
    public string? DupDescricaoDespesa { get; set; }

    // Navegação
    public virtual ICollection<Parcela> Parcelas { get; set; } = new List<Parcela>();
}
