using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_CCU_CENTRO_CUSTO")]
public class CentroCusto
{
    [Key]
    [Column("CCUID")]
    public int CcuId { get; set; }

    [Column("EMPID")]
    public int EmpId { get; set; }

    [ForeignKey(nameof(EmpId))]
    public Empresa? Empresa { get; set; }
}
