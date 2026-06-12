using Application.DTOs;

namespace Application.Interfaces;

public interface IRelatorioGerencialService
{
    Task<RelatorioGerencialResponseDto> ObterRelatorioAsync(DateTime dataInicio, DateTime dataFim, string tipoRelatorio);
}
