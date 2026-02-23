using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CAD_STATUS_ATEND_COMERCIAL")]
public class CadastroStatusAtendimentoComercial
{
    [Key]
    [Column("SACID")]
    public int Id { get; set; }

    [Column("SACNUMERO")]
    public int Numero { get; set; }

    [Column("SACDESCRICAO")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Column("SACATIVO")]
    public bool Ativo { get; set; } = true;
}
