using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_USU_USUARIO")]
public class Usuario
{
    [Key]
    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("PESID")]
    public int PesId { get; set; }

    [Column("USULOGIN")]
    [MaxLength(255)]
    public string? UsuLogin { get; set; }

    [Column("USUSENHA")]
    [MaxLength(255)]
    public string? UsuSenha { get; set; }

    [Column("USUPERFIL")]
    public int UsuPerfil { get; set; } = (int)PerfilUsuario.Comercial;

    // Navegação
    [ForeignKey("PesId")]
    public virtual Pessoa Pessoa { get; set; } = null!;

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
    public virtual ICollection<AnotacaoTarefa> AnotacoesTarefas { get; set; } = new List<AnotacaoTarefa>();
}
