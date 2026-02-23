using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CAD_TIPO_CONTATO")]
public class CadastroTipoContato
{
    [Key]
    [Column("TCID")]
    public int Id { get; set; }

    [Column("TCDESCRICAO")]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Column("TCATIVO")]
    public bool Ativo { get; set; } = true;
}
