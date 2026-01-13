namespace Application.DTOs;

public class UsuarioResponseDto
{
    public int UsuarioId { get; set; }
    public int PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Sobrenome { get; set; }
    public string? DocFederal { get; set; }
    public string? DocEstadual { get; set; }
    public string Login { get; set; } = string.Empty;
}
