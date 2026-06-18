using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Destinatários de campanhas de e-mail comercial.
/// </summary>
[Table("TB_EMAIL_ENVIO_COMERCIAL")]
public class EmailEnvioComercial
{
    [Key]
    [Column("EECID")]
    public int EecId { get; set; }

    [Column("EECEMAIL")]
    [MaxLength(255)]
    public string EecEmail { get; set; } = string.Empty;

    /// <summary>
    /// Quando verdadeiro, o destinatário é ignorado nos envios.
    /// </summary>
    [Column("EECNAOENVIAR")]
    public bool EecNaoEnviar { get; set; }

    [Column("EECDATACADASTRO")]
    public DateTime EecDataCadastro { get; set; } = DateTime.UtcNow;
}
