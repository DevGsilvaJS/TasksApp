using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace Domain.Entities;



[Table("TB_DUP_DUPLICATA")]

public class Duplicata

{

    [Key]

    [Column("DUPID")]

    public int DupId { get; set; }



    [Column("DUPNUMERO")]

    public int DupNumero { get; set; }



    [Column("DUPDATAEMISSAO")]

    public DateTime DupDataEmissao { get; set; }



    [Column("DUPNUMEROPARCELAS")]

    public int DupNumeroParcelas { get; set; }



    [Column("DUPDESCRICAODESPESA")]

    [MaxLength(500)]

    public string? DupDescricaoDespesa { get; set; }



    [Column("DUPTIPO")]

    [MaxLength(2)]

    [Required]

    public string DupTipo { get; set; } = "CP"; // CP = Contas a Pagar, CR = Contas a Receber



    [Column("CLIID")]

    public int? CliId { get; set; }



    [Column("CCUID")]
    public int? CcuId { get; set; }

    [Column("EMPID")]

    public int? EmpId { get; set; }



    [Column("PLCID")]

    public int? PlcId { get; set; }



    [ForeignKey("CliId")]

    public virtual Cliente? Cliente { get; set; }



    [ForeignKey(nameof(EmpId))]

    public virtual Empresa? Empresa { get; set; }



    [ForeignKey(nameof(PlcId))]

    public virtual PlanoContas? PlanoContas { get; set; }



    public virtual ICollection<Parcela> Parcelas { get; set; } = new List<Parcela>();

}

