using System.Globalization;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/relatorio-gerencial")]
public class RelatorioGerencialController : ControllerBase
{
    private readonly IRelatorioGerencialService _relatorioService;
    private readonly ILogger<RelatorioGerencialController> _logger;

    public RelatorioGerencialController(
        IRelatorioGerencialService relatorioService,
        ILogger<RelatorioGerencialController> logger)
    {
        _relatorioService = relatorioService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObterRelatorio(
        [FromQuery] string? dataInicio,
        [FromQuery] string? dataFim,
        [FromQuery] string? tipo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return BadRequest(new { message = "Informe o tipo de relatório." });

            if (!TryParseDataConsulta(dataInicio, out var inicio))
                return BadRequest(new { message = "Data inicial inválida. Use o formato AAAA-MM-DD." });

            if (!TryParseDataConsulta(dataFim, out var fim))
                return BadRequest(new { message = "Data final inválida. Use o formato AAAA-MM-DD." });

            var relatorio = await _relatorioService.ObterRelatorioAsync(inicio, fim, tipo.Trim());
            return Ok(relatorio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório gerencial. Tipo: {Tipo}", tipo);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private static bool TryParseDataConsulta(string? valor, out DateTime data)
    {
        data = default;
        if (string.IsNullOrWhiteSpace(valor)) return false;

        return DateTime.TryParseExact(
            valor.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out data);
    }
}
