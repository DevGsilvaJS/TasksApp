using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém estatísticas do dashboard para um período
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardEstatisticasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterEstatisticas([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        try
        {
            DateTime inicio;
            DateTime fim;

            // Se não fornecido, usar data atual
            if (!dataInicio.HasValue || !dataFim.HasValue)
            {
                var hoje = DateTime.Today;
                inicio = hoje;
                fim = hoje;
            }
            else
            {
                inicio = dataInicio.Value;
                fim = dataFim.Value;
            }

            var estatisticas = await _dashboardService.ObterEstatisticasAsync(inicio, fim);
            return Ok(estatisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas do dashboard");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém valores de contratos por mês por usuário
    /// </summary>
    [HttpGet("valores-por-mes-usuario")]
    [ProducesResponseType(typeof(List<ValorPorMesPorUsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterValoresPorMesPorUsuario([FromQuery] int? ano)
    {
        try
        {
            var valores = await _dashboardService.ObterValoresPorMesPorUsuarioAsync(ano);
            return Ok(valores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter valores por mês por usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
