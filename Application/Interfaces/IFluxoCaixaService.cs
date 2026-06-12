using Application.DTOs;

namespace Application.Interfaces;

public interface IFluxoCaixaService
{
    Task<FluxoCaixaResponseDto> ObterFluxoCaixaPorAnoAsync(int ano);
}
