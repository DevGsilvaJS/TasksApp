using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Possíveis clientes importados da planilha Excel para trabalho de ligação.
/// Colunas: Código (C), Loja (D), Status (E), Fantasia (F), DDD (I), CNPJ (Q), Razão Social (R),
/// E-mail comercial (AA), Cel DDD (AC), Celular (AD).
/// </summary>
[Table("TB_POSSIVEL_CLIENTE")]
public class PossivelCliente
{
    [Key]
    [Column("POCID")]
    public int PocId { get; set; }

    [Column("POCCODIGO")]
    [MaxLength(50)]
    public string PocCodigo { get; set; } = string.Empty;

    [Column("POCLOJA")]
    [MaxLength(200)]
    public string? PocLoja { get; set; }

    [Column("POCSTATUS")]
    [MaxLength(100)]
    public string? PocStatus { get; set; }

    [Column("POCFANTASIA")]
    [MaxLength(300)]
    public string? PocFantasia { get; set; }

    [Column("POCDDD")]
    [MaxLength(20)]
    public string? PocDdd { get; set; }

    [Column("POCCNPJ")]
    [MaxLength(20)]
    public string? PocCnpj { get; set; }

    [Column("POCRAZAOSOCIAL")]
    [MaxLength(500)]
    public string? PocRazaoSocial { get; set; }

    [Column("POCEMAILCOMERCIAL")]
    [MaxLength(200)]
    public string? PocEmailComercial { get; set; }

    [Column("POCCELDDD")]
    [MaxLength(20)]
    public string? PocCelDdd { get; set; }

    [Column("POCCELULAR")]
    [MaxLength(50)]
    public string? PocCelular { get; set; }

    [Column("POCDATAIMPORTACAO")]
    public DateTime? PocDataImportacao { get; set; }

    /// <summary>Status de atendimento comercial (1 a 9).</summary>
    [Column("POC_STATUS_ATENDIMENTO")]
    public int? PocStatusAtendimento { get; set; }

    [Column("POC_MOTIVO_PERDA")]
    [MaxLength(500)]
    public string? PocMotivoPerda { get; set; }

    [Column("POC_DATA_STATUS_ATENDIMENTO")]
    public DateTime? PocDataStatusAtendimento { get; set; }
}
