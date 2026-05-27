using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CLI_CONTRATO_VALOR")]
public class ClienteContratoValor
{
    [Key]
    [Column("CVCID")]
    public int CvcId { get; set; }

    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("CVCVALORMENSAL", TypeName = "decimal(18,2)")]
    public decimal CvcValorMensal { get; set; }

    [Column("CVCDATAINICIO")]
    public DateTime CvcDataInicio { get; set; }

    [Column("CVCDATAFIM")]
    public DateTime? CvcDataFim { get; set; }

    [ForeignKey("CliId")]
    public virtual Cliente Cliente { get; set; } = null!;
}

