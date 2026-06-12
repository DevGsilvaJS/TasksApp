using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FluxoCaixaController : ControllerBase
{
    private readonly IFluxoCaixaService _fluxoCaixaService;
    private readonly ILogger<FluxoCaixaController> _logger;

    public FluxoCaixaController(IFluxoCaixaService fluxoCaixaService, ILogger<FluxoCaixaController> logger)
    {
        _fluxoCaixaService = fluxoCaixaService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(FluxoCaixaResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterFluxoCaixa([FromQuery] int? ano)
    {
        try
        {
            var anoConsulta = ano ?? DateTime.UtcNow.Year;
            var resultado = await _fluxoCaixaService.ObterFluxoCaixaPorAnoAsync(anoConsulta);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fluxo de caixa para o ano {Ano}", ano);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
