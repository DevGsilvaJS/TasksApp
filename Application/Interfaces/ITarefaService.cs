using Application.DTOs;

namespace Application.Interfaces;

public interface ITarefaService
{
    Task<TarefaResponseDto> CadastrarTarefaAsync(CadastroTarefaDto dto);
    Task<TarefaResponseDto?> ObterTarefaPorIdAsync(int id);
    Task<IEnumerable<TarefaResponseDto>> ListarTodasTarefasAsync();
    /// <summary>
    /// Lista tarefas com filtros opcionais no banco.
    /// </summary>
    /// <param name="usuarioId">Se informado, retorna apenas tarefas deste usuário; se null, retorna de todos.</param>
    /// <param name="incluirConcluidas">Se true, inclui concluídas; se false, exclui status Concluída.</param>
    /// <param name="criterio">titulo | cliente | status | numero | data | executor</param>
    /// <param name="valor">Valor da pesquisa (LIKE %valor% para textos).</param>
    Task<IEnumerable<TarefaResponseDto>> ListarTarefasAsync(
        int? usuarioId,
        bool incluirConcluidas,
        string? criterio = null,
        string? valor = null);
    Task<TarefaResponseDto> AtualizarTarefaAsync(int id, CadastroTarefaDto dto);
    Task ExcluirTarefaAsync(int id);
    Task<TarefaResponseDto> AlterarStatusTarefaAsync(int id, int novoStatus);
}
