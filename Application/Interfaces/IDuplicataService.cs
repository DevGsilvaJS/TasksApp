using Application.DTOs;

namespace Application.Interfaces;

public interface IDuplicataService
{
    Task<DuplicataResponseDto> CadastrarDuplicataAsync(CadastroDuplicataDto dto);
    Task<DuplicataResponseDto?> ObterDuplicataPorIdAsync(int id);
    Task<IEnumerable<DuplicataResponseDto>> ListarTodasDuplicatasAsync();
    Task<DuplicataResponseDto> AtualizarDuplicataAsync(int id, CadastroDuplicataDto dto);
    Task ExcluirDuplicataAsync(int id);
    Task<ParcelaResponseDto> BaixarParcelaAsync(int parcelaId);
}
