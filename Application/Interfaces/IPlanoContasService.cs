using Application.DTOs;

namespace Application.Interfaces;

public interface IPlanoContasService
{
    Task<PlanoContasResponseDto> CadastrarPlanoContasAsync(CadastroPlanoContasDto dto);
    Task<PlanoContasResponseDto?> ObterPlanoContasPorIdAsync(int id);
    Task<IEnumerable<PlanoContasResponseDto>> ListarTodosPlanosContasAsync();
    Task<PlanoContasResponseDto> AtualizarPlanoContasAsync(int id, CadastroPlanoContasDto dto);
    Task ExcluirPlanoContasAsync(int id);
}
