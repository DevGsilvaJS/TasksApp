using Application.DTOs;

namespace Application.Interfaces;

public interface ITarefaService
{
    Task<TarefaResponseDto> CadastrarTarefaAsync(CadastroTarefaDto dto);
    Task<TarefaResponseDto?> ObterTarefaPorIdAsync(int id);
    Task<IEnumerable<TarefaResponseDto>> ListarTodasTarefasAsync();
    /// <summary>
    /// Lista tarefas com filtros opcionais: por usuário e inclusão de concluídas.
    /// </summary>
    /// <param name="usuarioId">Se informado, retorna apenas tarefas deste usuário; se null, retorna de todos.</param>
    /// <param name="incluirConcluidas">Se true, inclui tarefas concluídas; se false, exclui.</param>
    Task<IEnumerable<TarefaResponseDto>> ListarTarefasAsync(int? usuarioId, bool incluirConcluidas);
    Task<TarefaResponseDto> AtualizarTarefaAsync(int id, CadastroTarefaDto dto);
    Task ExcluirTarefaAsync(int id);
    Task<TarefaResponseDto> AlterarStatusTarefaAsync(int id, Domain.Enums.StatusTarefa novoStatus);
}
