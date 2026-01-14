using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("TB_TAR_TAREFAS")]
public class Tarefa
{
    [Key]
    [Column("TARID")]
    public int TarId { get; set; }

    [Column("CLIID")]
    public int CliId { get; set; }

    [Column("USUID")]
    public int UsuId { get; set; }

    [Column("TARDTCADASTRO")]
    public DateTime? TarDtCadastro { get; set; }

    [Column("TARDTCONCLUSAO")]
    public DateTime? TarDtConclusao { get; set; }

    [Column("TARSTATUS")]
    public StatusTarefa TarStatus { get; set; } = StatusTarefa.EmAberto;

    [Column("TARTITULO")]
    [MaxLength(255)]
    public string? TarTitulo { get; set; }

    [Column("TARPROTOCOLO")]
    [MaxLength(6)]
    public string? TarProtocolo { get; set; }

    [Column("TARSOLICITANTE")]
    [MaxLength(255)]
    public string? TarSolicitante { get; set; }

    [Column("TARCELULARSOLICITANTE")]
    [MaxLength(20)]
    public string? TarCelularSolicitante { get; set; }

    [Column("TARTIPOATENDIMENTO")]
    public TipoAtendimento? TarTipoAtendimento { get; set; }

    [Column("TARPRIORIDADE")]
    public PrioridadeTarefa TarPrioridade { get; set; } = PrioridadeTarefa.Media;

    [Column("TARNUMERO")]
    public int? TarNumero { get; set; }

    [Column("TARTIPOCONTATO")]
    public TipoContato? TarTipoContato { get; set; }

    // Navegação
    [ForeignKey("CliId")]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey("UsuId")]
    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<ImagemTarefa> ImagensTarefa { get; set; } = new List<ImagemTarefa>();
    public virtual ICollection<AnotacaoTarefa> AnotacoesTarefas { get; set; } = new List<AnotacaoTarefa>();
}
