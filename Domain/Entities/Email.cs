using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_EMAIL")]
public class Email
{
    [Key]
    [Column("EMLID")]
    public int EmlId { get; set; }

    [Column("PESID")]
    public int PesId { get; set; }

    [Column("EMLDESCRICAO")]
    [MaxLength(255)]
    public string? EmlDescricao { get; set; }

    // Navegação
    [ForeignKey("PesId")]
    public virtual Pessoa Pessoa { get; set; } = null!;
}
