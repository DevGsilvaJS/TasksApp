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
    Task<ParcelaResponseDto> BaixarParcelaAsync(int parcelaId);
    Task<ParcelaResponseDto> ReativarParcelaAsync(int parcelaId);
    Task<ParcelaResponseDto> AtualizarClassificacaoParcelaAsync(int parcelaId, AtualizarClassificacaoParcelaDto dto);
    Task<int> ObterProximoNumeroAsync(string tipo);
}
