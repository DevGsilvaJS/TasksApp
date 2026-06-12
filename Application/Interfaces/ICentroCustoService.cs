using Application.DTOs;

namespace Application.Interfaces;

public interface ICentroCustoService
{
    Task<CentroCustoResponseDto> CadastrarCentroCustoAsync(CadastroCentroCustoDto dto);
    Task<CentroCustoResponseDto?> ObterCentroCustoPorIdAsync(int id);
    Task<IEnumerable<CentroCustoResponseDto>> ListarTodosCentrosCustoAsync();
    Task<CentroCustoResponseDto> AtualizarCentroCustoAsync(int id, CadastroCentroCustoDto dto);
    Task ExcluirCentroCustoAsync(int id);
}
