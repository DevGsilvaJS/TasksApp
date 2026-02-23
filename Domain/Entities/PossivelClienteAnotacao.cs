using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_POSSIVEL_CLIENTE_ANOTACAO")]
public class PossivelClienteAnotacao
{
    [Key]
    [Column("PCAID")]
    public int PcaId { get; set; }

    [Column("POCID")]
    public int PocId { get; set; }

    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("PCADESCRICAO")]
    [MaxLength(3000)]
    public string? PcaDescricao { get; set; }

    [Column("PCADTCADASTRO")]
    public DateTime PcaDtCadastro { get; set; }

    [ForeignKey("PocId")]
    public virtual PossivelCliente PossivelCliente { get; set; } = null!;

    [ForeignKey("UsuId")]
    public virtual Usuario Usuario { get; set; } = null!;
}
