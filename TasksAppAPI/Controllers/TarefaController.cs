using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarefaController : ControllerBase
{
    private readonly ITarefaService _tarefaService;
    private readonly ILogger<TarefaController> _logger;

    public TarefaController(
        ITarefaService tarefaService,
        ILogger<TarefaController> logger)
    {
        _tarefaService = tarefaService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma nova tarefa
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarTarefa([FromForm] CadastroTarefaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tarefa = await _tarefaService.CadastrarTarefaAsync(dto);
            return CreatedAtAction(nameof(ObterTarefaPorId), new { id = tarefa.TarefaId }, tarefa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar tarefa");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma tarefa por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterTarefaPorId(int id)
    {
        try
        {
            var tarefa = await _tarefaService.ObterTarefaPorIdAsync(id);
            if (tarefa == null)
            {
                return NotFound(new { message = "Tarefa não encontrada" });
            }

            return Ok(tarefa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tarefa por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as tarefas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TarefaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodasTarefas()
    {
        try
        {
            var tarefas = await _tarefaService.ListarTodasTarefasAsync();
            return Ok(tarefas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tarefas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma tarefa
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarTarefa(int id, [FromForm] CadastroTarefaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tarefa = await _tarefaService.AtualizarTarefaAsync(id, dto);
            return Ok(tarefa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui uma tarefa
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirTarefa(int id)
    {
        try
        {
            await _tarefaService.ExcluirTarefaAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tarefa: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Altera o status de uma tarefa
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TarefaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatusTarefa(int id, [FromBody] AlterarStatusTarefaDto dto)
    {
        try
        {
            var tarefa = await _tarefaService.AlterarStatusTarefaAsync(id, dto.Status);
            return Ok(tarefa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status da tarefa: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}

public class AlterarStatusTarefaDto
{
    public Domain.Enums.StatusTarefa Status { get; set; }
}
