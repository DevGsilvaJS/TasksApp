using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CentroCustoController : ControllerBase
{
    private readonly ICentroCustoService _centroCustoService;
    private readonly ILogger<CentroCustoController> _logger;

    public CentroCustoController(ICentroCustoService centroCustoService, ILogger<CentroCustoController> logger)
    {
        _centroCustoService = centroCustoService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CentroCustoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarCentroCusto([FromBody] CadastroCentroCustoDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var centro = await _centroCustoService.CadastrarCentroCustoAsync(dto);
            return CreatedAtAction(nameof(ObterCentroCustoPorId), new { id = centro.CentroCustoId }, centro);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar centro de custo");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CentroCustoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCentroCustoPorId(int id)
    {
        try
        {
            var centro = await _centroCustoService.ObterCentroCustoPorIdAsync(id);
            if (centro == null) return NotFound(new { message = "Centro de custo não encontrado" });
            return Ok(centro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter centro de custo por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CentroCustoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodosCentrosCusto()
    {
        try
        {
            var centros = await _centroCustoService.ListarTodosCentrosCustoAsync();
            return Ok(centros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar centros de custo");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CentroCustoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarCentroCusto(int id, [FromBody] CadastroCentroCustoDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var centro = await _centroCustoService.AtualizarCentroCustoAsync(id, dto);
            return Ok(centro);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar centro de custo {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirCentroCusto(int id)
    {
        try
        {
            await _centroCustoService.ExcluirCentroCustoAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir centro de custo {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
