using Application.DTOs;

namespace Application.Interfaces;

public interface IPossivelClienteService
{
    Task<IEnumerable<PossivelClienteResponseDto>> ListarTodosAsync();
    Task<PossivelClienteResponseDto?> ObterPorIdAsync(int id);
    Task<PossivelClienteResponseDto?> AtualizarStatusAtendimentoAsync(int id, AtualizarStatusAtendimentoDto dto);
    Task<IEnumerable<PossivelClienteAnotacaoResponseDto>> ListarAnotacoesAsync(int pocId);
    Task<PossivelClienteAnotacaoResponseDto> AdicionarAnotacaoAsync(int pocId, CadastroPossivelClienteAnotacaoDto dto);
    Task<PossivelClienteAnotacaoResponseDto?> AtualizarAnotacaoAsync(int pocId, int anotacaoId, AtualizarPossivelClienteAnotacaoDto dto);
}
