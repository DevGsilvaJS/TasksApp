using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CAD_STATUS_TAREFA")]
public class CadastroStatusTarefa
{
    [Key]
    [Column("STCID")]
    public int Id { get; set; }

    [Column("STCDESCRICAO")]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Column("STCATIVO")]
    public bool Ativo { get; set; } = true;
}
