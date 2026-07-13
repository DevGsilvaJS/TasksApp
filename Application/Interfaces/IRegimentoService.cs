using Application.DTOs;

namespace Application.Interfaces;

public interface IRegimentoService
{
    Task<RegimentoResponseDto> CadastrarRegimentoAsync(CadastroRegimentoDto dto);
    Task<RegimentoDetalheResponseDto?> ObterRegimentoPorIdAsync(int id, int? usuarioId = null);
    Task<IEnumerable<RegimentoResponseDto>> ListarRegimentosAsync();
    Task<RegimentoResponseDto> AtualizarRegimentoAsync(int id, CadastroRegimentoDto dto);
    Task ExcluirRegimentoAsync(int id);
    Task<RegimentoAceiteResponseDto> RegistrarAceiteAsync(int regimentoId, int usuarioId, CadastroRegimentoAceiteDto dto);
    Task DesfazerAceiteAsync(int aceiteId, int usuarioId);
    Task<IEnumerable<RegimentoAceiteLogResponseDto>> ListarLogAceitesAsync(int regimentoId);
}
