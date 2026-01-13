using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_PAR_PARCELA")]
public class Parcela
{
    [Key]
    [Column("PARID")]
    public int ParId { get; set; }

    [Column("DUPID")]
    public int DupId { get; set; }

    [Column("PAR_NUMERO_PARCELA")]
    public int ParNumeroParcela { get; set; }

    [Column("PARVALOR")]
    public double ParValor { get; set; }

    [Column("PARMULTA")]
    public double ParMulta { get; set; }

    [Column("PARJUROS")]
    public double ParJuros { get; set; }

    [Column("PARVENCIMENTO")]
    public DateTime ParVencimento { get; set; }

    [Column("PARSTATUS")]
    [MaxLength(50)]
    public string ParStatus { get; set; } = "Pendente"; // Pendente, Paga, Cancelada

    [Column("PAR_DATA_PAGAMENTO")]
    public DateTime? ParDataPagamento { get; set; }

    // Navegação
    [ForeignKey("DupId")]
    public virtual Duplicata Duplicata { get; set; } = null!;
}
