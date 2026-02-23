using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PossivelClienteController : ControllerBase
{
    private readonly IPossivelClienteService _service;
    private readonly ILogger<PossivelClienteController> _logger;

    public PossivelClienteController(
        IPossivelClienteService service,
        ILogger<PossivelClienteController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os possíveis clientes (importados da planilha).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Application.DTOs.PossivelClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        try
        {
            var lista = await _service.ListarTodosAsync();
            return Ok(lista);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar possíveis clientes");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um possível cliente por ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.DTOs.PossivelClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var item = await _service.ObterPorIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Possível cliente não encontrado" });
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter possível cliente {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza o status de atendimento comercial do possível cliente (1 a 9).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PossivelClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatusAtendimento(int id, [FromBody] AtualizarStatusAtendimentoDto dto)
    {
        try
        {
            var item = await _service.AtualizarStatusAtendimentoAsync(id, dto);
            if (item == null)
                return NotFound(new { message = "Possível cliente não encontrado" });
            return Ok(item);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status de atendimento {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista anotações de atendimento do possível cliente.
    /// </summary>
    [HttpGet("{id}/anotacoes")]
    [ProducesResponseType(typeof(IEnumerable<PossivelClienteAnotacaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAnotacoes(int id)
    {
        try
        {
            var lista = await _service.ListarAnotacoesAsync(id);
            return Ok(lista);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar anotações do possível cliente {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Adiciona uma anotação de atendimento ao possível cliente.
    /// </summary>
    [HttpPost("{id}/anotacoes")]
    [ProducesResponseType(typeof(PossivelClienteAnotacaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdicionarAnotacao(int id, [FromBody] CadastroPossivelClienteAnotacaoDto dto)
    {
        try
        {
            var anotacao = await _service.AdicionarAnotacaoAsync(id, dto);
            return CreatedAtAction(nameof(ListarAnotacoes), new { id }, anotacao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar anotação ao possível cliente {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma anotação de atendimento do possível cliente.
    /// </summary>
    [HttpPut("{id}/anotacoes/{anotacaoId}")]
    [ProducesResponseType(typeof(PossivelClienteAnotacaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarAnotacao(int id, int anotacaoId, [FromBody] AtualizarPossivelClienteAnotacaoDto dto)
    {
        try
        {
            var anotacao = await _service.AtualizarAnotacaoAsync(id, anotacaoId, dto);
            if (anotacao == null)
                return NotFound(new { message = "Anotação não encontrada." });
            return Ok(anotacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar anotação {AnotacaoId} do possível cliente {Id}", anotacaoId, id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
