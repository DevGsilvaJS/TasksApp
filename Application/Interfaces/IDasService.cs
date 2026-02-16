using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces;

public interface IDasService
{
    Task<DasResponseDto> CadastrarAsync(CadastroDasDto dto);
    Task<DasResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<DasResponseDto>> ListarTodasAsync();
    Task<DasResponseDto> AtualizarAsync(int id, CadastroDasDto dto);
    Task<DasResponseDto?> AtualizarStatusAsync(int id, StatusDas status);
    Task ExcluirAsync(int id);
}
