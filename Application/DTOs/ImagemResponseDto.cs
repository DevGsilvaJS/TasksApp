namespace Application.DTOs;

public class ImagemResponseDto
{
    public int ImagemId { get; set; }
    public int TarefaId { get; set; }
    public string UrlImagem { get; set; } = string.Empty;
    public DateTime? DataArquivo { get; set; }
}
