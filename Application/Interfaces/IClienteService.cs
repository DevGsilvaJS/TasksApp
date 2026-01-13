using Application.DTOs;

namespace Application.Interfaces;

public interface IClienteService
{
    Task<ClienteResponseDto> CadastrarClienteAsync(CadastroClienteDto dto);
    Task<ClienteResponseDto?> ObterClientePorIdAsync(int id);
    Task<IEnumerable<ClienteResponseDto>> ListarTodosClientesAsync();
    Task<ClienteResponseDto> AtualizarClienteAsync(int id, CadastroClienteDto dto);
    Task ExcluirClienteAsync(int id);
}
