using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_PESSOA")]
public class Pessoa
{
    [Key]
    [Column("PESID")]
    public int PesId { get; set; }

    [Column("PESFANTASIA")]
    [MaxLength(255)]
    public string? PesFantasia { get; set; }

    [Column("PESDOCFEDERAL")]
    [MaxLength(255)]
    public string? PesDocFederal { get; set; }

    [Column("PESDOCESTADUAL")]
    [MaxLength(255)]
    public string? PesDocEstadual { get; set; }

    // Navegação
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
    public virtual Usuario? Usuario { get; set; }
}
