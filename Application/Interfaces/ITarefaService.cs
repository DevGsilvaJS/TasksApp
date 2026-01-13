using Application.DTOs;

namespace Application.Interfaces;

public interface ITarefaService
{
    Task<TarefaResponseDto> CadastrarTarefaAsync(CadastroTarefaDto dto);
    Task<TarefaResponseDto?> ObterTarefaPorIdAsync(int id);
    Task<IEnumerable<TarefaResponseDto>> ListarTodasTarefasAsync();
    Task<TarefaResponseDto> AtualizarTarefaAsync(int id, CadastroTarefaDto dto);
    Task ExcluirTarefaAsync(int id);
    Task<TarefaResponseDto> AlterarStatusTarefaAsync(int id, Domain.Enums.StatusTarefa novoStatus);
}
