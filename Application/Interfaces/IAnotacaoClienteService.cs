using Application.DTOs;

namespace Application.Interfaces;

public interface IAnotacaoClienteService
{
    Task<AnotacaoClienteResponseDto> CadastrarAsync(CadastroAnotacaoClienteDto dto);
    Task<AnotacaoClienteResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<AnotacaoClienteResponseDto>> ListarPorClienteAsync(int clienteId);
    Task<IEnumerable<AnotacaoClienteResponseDto>> ListarTodasAsync();
    Task<AnotacaoClienteResponseDto> AtualizarAsync(int id, CadastroAnotacaoClienteDto dto);
    Task ExcluirAsync(int id);
}
