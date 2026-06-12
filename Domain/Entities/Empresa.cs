using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_EMP_EMPRESA")]
public class Empresa
{
    [Key]
    [Column("EMPID")]
    public int EmpId { get; set; }

    [Column("EMPCNPJ")]
    [MaxLength(20)]
    public string EmpCnpj { get; set; } = string.Empty;

    [Column("EMPRAZAOSOCIAL")]
    [MaxLength(500)]
    public string EmpRazaoSocial { get; set; } = string.Empty;

    [Column("EMPFANTASIA")]
    [MaxLength(300)]
    public string EmpFantasia { get; set; } = string.Empty;

    public ICollection<CentroCusto> CentrosCusto { get; set; } = new List<CentroCusto>();
}
