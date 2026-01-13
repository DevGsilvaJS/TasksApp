using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("TB_IMT_IMAGEM_TAREFA")]
public class ImagemTarefa
{
    [Key]
    [Column("IMGID")]
    public int ImgId { get; set; }

    [Column("TARID")]
    public int TarId { get; set; }

    [Column("IMGARQUIVO")]
    public byte[]? ImgArquivo { get; set; }

    [Column("IMGDATAARQUIVO")]
    public DateTime? ImgDataArquivo { get; set; }

    // Navegação
    [ForeignKey("TarId")]
    public virtual Tarefa Tarefa { get; set; } = null!;
}
