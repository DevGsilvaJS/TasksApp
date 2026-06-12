using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CAD_ANDAMENTO")]
public class CadastroAndamento
{
    [Key]
    [Column("ANID")]
    public int Id { get; set; }

    [Column("ANDESCRICAO")]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Column("ANATIVO")]
    public bool Ativo { get; set; } = true;
}
