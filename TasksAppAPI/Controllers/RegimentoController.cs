using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegimentoController : ControllerBase
{
    private readonly IRegimentoService _regimentoService;
    private readonly ILogger<RegimentoController> _logger;

    public RegimentoController(IRegimentoService regimentoService, ILogger<RegimentoController> logger)
    {
        _regimentoService = regimentoService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RegimentoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarRegimento([FromBody] CadastroRegimentoDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var regimento = await _regimentoService.CadastrarRegimentoAsync(dto);
            return CreatedAtAction(nameof(ObterRegimentoPorId), new { id = regimento.RegimentoId }, regimento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar regimento");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegimentoDetalheResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterRegimentoPorId(int id, [FromQuery] int? usuarioId = null)
    {
        try
        {
            var regimento = await _regimentoService.ObterRegimentoPorIdAsync(id, usuarioId);
            if (regimento == null) return NotFound(new { message = "Regimento não encontrado" });
            return Ok(regimento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter regimento por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegimentoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRegimentos()
    {
        try
        {
            var regimentos = await _regimentoService.ListarRegimentosAsync();
            return Ok(regimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar regimentos");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RegimentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarRegimento(int id, [FromBody] CadastroRegimentoDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var regimento = await _regimentoService.AtualizarRegimentoAsync(id, dto);
            return Ok(regimento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar regimento {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirRegimento(int id)
    {
        try
        {
            await _regimentoService.ExcluirRegimentoAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir regimento {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("{id}/aceite")]
    [ProducesResponseType(typeof(RegimentoAceiteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarAceite(int id, [FromQuery] int usuarioId, [FromBody] CadastroRegimentoAceiteDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var aceite = await _regimentoService.RegistrarAceiteAsync(id, usuarioId, dto);
            return CreatedAtAction(nameof(ObterRegimentoPorId), new { id }, aceite);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar aceite do regimento {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("aceite/{aceiteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DesfazerAceite(int aceiteId, [FromQuery] int usuarioId)
    {
        try
        {
            await _regimentoService.DesfazerAceiteAsync(aceiteId, usuarioId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desfazer aceite {AceiteId}", aceiteId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}/log")]
    [ProducesResponseType(typeof(IEnumerable<RegimentoAceiteLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarLogAceites(int id)
    {
        try
        {
            var logs = await _regimentoService.ListarLogAceitesAsync(id);
            return Ok(logs);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar log de aceites do regimento {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
