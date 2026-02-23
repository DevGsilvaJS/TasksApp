using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CAD_TIPO_ATENDIMENTO")]
public class CadastroTipoAtendimento
{
    [Key]
    [Column("TAID")]
    public int Id { get; set; }

    [Column("TADESCRICAO")]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Column("TAATIVO")]
    public bool Ativo { get; set; } = true;
}
