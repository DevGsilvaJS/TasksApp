namespace Application.DTOs;

public class LoginResponseDto
{
    public int UsuarioId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Autenticado { get; set; }
}
