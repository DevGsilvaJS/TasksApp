using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DasController : ControllerBase
{
    private readonly IDasService _dasService;
    private readonly ILogger<DasController> _logger;

    public DasController(IDasService dasService, ILogger<DasController> logger)
    {
        _dasService = dasService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma guia DAS
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DasResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroDasDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _dasService.CadastrarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.DasId }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar DAS");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma DAS por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var resultado = await _dasService.ObterPorIdAsync(id);
            if (resultado == null)
                return NotFound(new { message = "DAS não encontrada" });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter DAS: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as DAS
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DasResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodas()
    {
        try
        {
            var resultado = await _dasService.ListarTodasAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar DAS");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma DAS
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CadastroDasDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _dasService.AtualizarAsync(id, dto);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar DAS: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza apenas o status da DAS (Pendente, EmDia, Atrasado)
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(DasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusDasDto dto)
    {
        try
        {
            var resultado = await _dasService.AtualizarStatusAsync(id, dto.Status);
            if (resultado == null)
                return NotFound(new { message = "DAS não encontrada" });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status da DAS: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui uma DAS
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _dasService.ExcluirAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir DAS: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
