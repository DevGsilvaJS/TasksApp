using Application.DTOs;

namespace Application.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioResponseDto> CadastrarUsuarioAsync(CadastroUsuarioDto dto);
    Task<UsuarioResponseDto?> ObterUsuarioPorIdAsync(int id);
    Task<UsuarioResponseDto?> ObterUsuarioPorLoginAsync(string login);
    Task<IEnumerable<UsuarioResponseDto>> ListarTodosUsuariosAsync();
    Task<LoginResponseDto> AutenticarAsync(LoginDto dto);
}
