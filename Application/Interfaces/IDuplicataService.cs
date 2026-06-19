using Application.DTOs;

namespace Application.Interfaces;

public interface IDuplicataService
{
    Task<DuplicataResponseDto> CadastrarDuplicataAsync(CadastroDuplicataDto dto);
    Task<DuplicataResponseDto?> ObterDuplicataPorIdAsync(int id);
    Task<IEnumerable<DuplicataResponseDto>> ListarTodasDuplicatasAsync();
    Task<IEnumerable<DuplicataResponseDto>> ListarDuplicatasPorTipoAsync(string tipo);
    Task<DuplicataResponseDto> AtualizarDuplicataAsync(int id, CadastroDuplicataDto dto);
    Task ExcluirDuplicataAsync(int id);
    Task<ParcelaResponseDto> BaixarParcelaAsync(int parcelaId, BaixarParcelaDto? dto = null);
    Task<ParcelaResponseDto> ReativarParcelaAsync(int parcelaId);
    Task<ParcelaResponseDto> InativarParcelaAsync(int parcelaId);
    Task<DuplicataResponseDto> InativarParcelasRestantesAsync(int duplicataId);
    Task<ParcelaResponseDto> ReativarParcelaInativaAsync(int parcelaId);
    Task<ParcelaResponseDto> AtualizarClassificacaoParcelaAsync(int parcelaId, AtualizarClassificacaoParcelaDto dto);
    Task<int> ObterProximoNumeroAsync(string tipo);
}
