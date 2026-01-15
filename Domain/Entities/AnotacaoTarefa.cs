using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_ANT_ANOTACOES_TAREFAS")]
public class AnotacaoTarefa
{
    [Key]
    [Column("ANTID")]
    public int AntId { get; set; }

    [Column("TARID")]
    public int TarId { get; set; }

    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("ANTDESCRICAO")]
    [MaxLength(3000)]
    public string? AntDescricao { get; set; }

    [Column("ANTDTCADASTRO")]
    public DateTime? AntDtCadastro { get; set; }

    // Navegação
    [ForeignKey("TarId")]
    public virtual Tarefa Tarefa { get; set; } = null!;

    [ForeignKey("UsuId")]
    public virtual Usuario Usuario { get; set; } = null!;
}
