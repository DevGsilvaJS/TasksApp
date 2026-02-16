using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotaServicoController : ControllerBase
{
    private readonly INotaServicoService _notaServicoService;
    private readonly ILogger<NotaServicoController> _logger;

    public NotaServicoController(INotaServicoService notaServicoService, ILogger<NotaServicoController> logger)
    {
        _notaServicoService = notaServicoService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as notas de serviço do mês (pendentes e enviadas) para o card do dashboard.
    /// </summary>
    [HttpGet("mes-atual")]
    [ProducesResponseType(typeof(List<NotaServicoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarNotasDoMesAtual()
    {
        try
        {
            var now = DateTime.UtcNow;
            var resultado = await _notaServicoService.ListarNotasDoMesAsync(now.Year, now.Month);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notas do mês");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista notas do mês por ano e mês.
    /// </summary>
    [HttpGet("mes/{ano}/{mes}")]
    [ProducesResponseType(typeof(List<NotaServicoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarNotasDoMes(int ano, int mes)
    {
        try
        {
            if (mes < 1 || mes > 12) return BadRequest(new { message = "Mês inválido (1-12)." });
            var resultado = await _notaServicoService.ListarNotasDoMesAsync(ano, mes);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notas do mês {Ano}/{Mes}", ano, mes);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Marca a nota de serviço do cliente no mês como enviada.
    /// </summary>
    [HttpPatch("marcar-enviado/{clienteId}/{ano}/{mes}")]
    [ProducesResponseType(typeof(NotaServicoItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoEnviado(int clienteId, int ano, int mes, [FromBody] MarcarNotaServicoEnviadoDto? dto = null)
    {
        try
        {
            if (mes < 1 || mes > 12) return BadRequest(new { message = "Mês inválido (1-12)." });
            var dataEnvio = dto?.DataEnvio?.ToUniversalTime();
            var resultado = await _notaServicoService.MarcarComoEnviadoAsync(clienteId, ano, mes, dataEnvio);
            if (resultado == null)
                return NotFound(new { message = "Cliente não encontrado ou sem dia de NF de serviço cadastrado." });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar nota como enviada: Cliente {ClienteId}, {Ano}/{Mes}", clienteId, ano, mes);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
