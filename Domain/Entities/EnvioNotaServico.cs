using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_ENS_ENVIO_NOTA_SERVICO")]
public class EnvioNotaServico
{
    [Key]
    [Column("ENSID")]
    public int EnsId { get; set; }

    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("ENSANO")]
    public int EnsAno { get; set; }

    [Column("ENSMES")]
    public int EnsMes { get; set; }

    /// <summary>
    /// Preenchido quando a nota foi enviada (marca como enviado).
    /// </summary>
    [Column("ENSDATAENVIO")]
    public DateTime? EnsDataEnvio { get; set; }

    [ForeignKey("CliId")]
    public virtual Cliente Cliente { get; set; } = null!;
}
