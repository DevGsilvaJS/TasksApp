using Application.DTOs;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardEstatisticasDto> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim);
}
