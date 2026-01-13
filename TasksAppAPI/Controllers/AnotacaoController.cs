using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnotacaoController : ControllerBase
{
    private readonly IAnotacaoService _anotacaoService;
    private readonly ILogger<AnotacaoController> _logger;

    public AnotacaoController(
        IAnotacaoService anotacaoService,
        ILogger<AnotacaoController> logger)
    {
        _anotacaoService = anotacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma nova anotação
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AnotacaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarAnotacao([FromBody] CadastroAnotacaoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anotacao = await _anotacaoService.CadastrarAnotacaoAsync(dto);
            return CreatedAtAction(nameof(ObterAnotacoesPorTarefa), new { tarefaId = anotacao.TarefaId }, anotacao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar anotação");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém anotações de uma tarefa
    /// </summary>
    [HttpGet("tarefa/{tarefaId}")]
    [ProducesResponseType(typeof(IEnumerable<AnotacaoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAnotacoesPorTarefa(int tarefaId)
    {
        try
        {
            var anotacoes = await _anotacaoService.ObterAnotacoesPorTarefaAsync(tarefaId);
            return Ok(anotacoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter anotações da tarefa: {TarefaId}", tarefaId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui uma anotação
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirAnotacao(int id)
    {
        try
        {
            await _anotacaoService.ExcluirAnotacaoAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir anotação: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
