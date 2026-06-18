using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_EMAIL_CAMPANHA_COMERCIAL")]
public class EmailCampanhaComercial
{
    [Key]
    [Column("ECCID")]
    public int EccId { get; set; }

    [Column("ECCASSUNTO")]
    [MaxLength(500)]
    public string EccAssunto { get; set; } = string.Empty;

    [Column("ECCCORPOHTML")]
    public string EccCorpoHtml { get; set; } = string.Empty;

    [Column("ECCSTATUS")]
    public StatusCampanhaEmailComercial EccStatus { get; set; } = StatusCampanhaEmailComercial.Fila;

    [Column("ECCDATACRIACAO")]
    public DateTime EccDataCriacao { get; set; } = DateTime.UtcNow;

    [Column("ECCPROXIMOINDICE")]
    public int EccProximoIndice { get; set; }

    [Column("ECCPAUSAATE")]
    public DateTime? EccPausaAte { get; set; }

    [Column("ECCTOTALITENS")]
    public int EccTotalItens { get; set; }

    [Column("ECCENVIADOS")]
    public int EccEnviados { get; set; }

    [Column("ECCERROS")]
    public int EccErros { get; set; }

    public virtual ICollection<EmailCampanhaItem> Itens { get; set; } = new List<EmailCampanhaItem>();
}
