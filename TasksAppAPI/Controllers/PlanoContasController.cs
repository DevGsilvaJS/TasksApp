using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanoContasController : ControllerBase
{
    private readonly IPlanoContasService _planoContasService;
    private readonly ILogger<PlanoContasController> _logger;

    public PlanoContasController(IPlanoContasService planoContasService, ILogger<PlanoContasController> logger)
    {
        _planoContasService = planoContasService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlanoContasResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarPlanoContas([FromBody] CadastroPlanoContasDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var plano = await _planoContasService.CadastrarPlanoContasAsync(dto);
            return CreatedAtAction(nameof(ObterPlanoContasPorId), new { id = plano.PlanoContasId }, plano);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar plano de contas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlanoContasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPlanoContasPorId(int id)
    {
        try
        {
            var plano = await _planoContasService.ObterPlanoContasPorIdAsync(id);
            if (plano == null) return NotFound(new { message = "Plano de contas não encontrado" });
            return Ok(plano);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter plano de contas por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlanoContasResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodosPlanosContas()
    {
        try
        {
            var planos = await _planoContasService.ListarTodosPlanosContasAsync();
            return Ok(planos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar planos de contas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PlanoContasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarPlanoContas(int id, [FromBody] CadastroPlanoContasDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var plano = await _planoContasService.AtualizarPlanoContasAsync(id, dto);
            return Ok(plano);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar plano de contas {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirPlanoContas(int id)
    {
        try
        {
            await _planoContasService.ExcluirPlanoContasAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir plano de contas {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
