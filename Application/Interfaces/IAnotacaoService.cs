using Application.DTOs;

namespace Application.Interfaces;

public interface IAnotacaoService
{
    Task<AnotacaoResponseDto> CadastrarAnotacaoAsync(CadastroAnotacaoDto dto);
    Task<IEnumerable<AnotacaoResponseDto>> ObterAnotacoesPorTarefaAsync(int tarefaId);
    Task ExcluirAnotacaoAsync(int id);
}
