using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertasController : ControllerBase
{
    private readonly IAlertasService _alertasService;
    private readonly ILogger<AlertasController> _logger;

    public AlertasController(IAlertasService alertasService, ILogger<AlertasController> logger)
    {
        _alertasService = alertasService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna pendências para pop-up: enviar notas de serviços e pagar DAS.
    /// </summary>
    /// <param name="diasParaAlertaNota">Alertar se a última nota foi enviada há mais de X dias (padrão: 30).</param>
    [HttpGet("pendencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPendencias([FromQuery] int diasParaAlertaNota = 30)
    {
        try
        {
            if (diasParaAlertaNota < 1)
                diasParaAlertaNota = 30;

            var resultado = await _alertasService.ObterPendenciasAsync(diasParaAlertaNota);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pendências para alertas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
