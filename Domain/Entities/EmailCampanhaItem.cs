using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_EMAIL_CAMPANHA_ITEM")]
public class EmailCampanhaItem
{
    [Key]
    [Column("ECIID")]
    public int EciId { get; set; }

    [Column("ECCID")]
    public int EccId { get; set; }

    [Column("EECID")]
    public int? EecId { get; set; }

    [Column("ECIEMAIL")]
    [MaxLength(255)]
    public string EciEmail { get; set; } = string.Empty;

    [Column("ECIORDEM")]
    public int EciOrdem { get; set; }

    [Column("ECISTATUS")]
    public StatusItemCampanhaEmail EciStatus { get; set; } = StatusItemCampanhaEmail.Pendente;

    [Column("ECIDATAENVIO")]
    public DateTime? EciDataEnvio { get; set; }

    [Column("ECIMENSAGEMERRO")]
    [MaxLength(1000)]
    public string? EciMensagemErro { get; set; }

    [Column("ECIREMETENTEEMAIL")]
    [MaxLength(255)]
    public string? EciRemetenteEmail { get; set; }

    [ForeignKey("EccId")]
    public virtual EmailCampanhaComercial Campanha { get; set; } = null!;

    [ForeignKey("EecId")]
    public virtual EmailEnvioComercial? Destinatario { get; set; }
}
