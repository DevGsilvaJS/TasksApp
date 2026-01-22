using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/anotacao-geral")]
public class AnotacaoGeralController : ControllerBase
{
    private readonly IAnotacaoGeralService _anotacaoGeralService;
    private readonly ILogger<AnotacaoGeralController> _logger;

    public AnotacaoGeralController(
        IAnotacaoGeralService anotacaoGeralService,
        ILogger<AnotacaoGeralController> logger)
    {
        _anotacaoGeralService = anotacaoGeralService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as anotações
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnotacaoGeralResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodasAnotacoes()
    {
        try
        {
            var anotacoes = await _anotacaoGeralService.ListarTodasAnotacoesAsync();
            return Ok(anotacoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar anotações");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma anotação por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnotacaoGeralResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterAnotacaoPorId(int id)
    {
        try
        {
            var anotacao = await _anotacaoGeralService.ObterAnotacaoPorIdAsync(id);
            if (anotacao == null)
                return NotFound(new { message = "Anotação não encontrada" });

            return Ok(anotacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter anotação: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cadastra uma nova anotação
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AnotacaoGeralResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarAnotacao([FromBody] CadastroAnotacaoGeralDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var anotacao = await _anotacaoGeralService.CadastrarAnotacaoAsync(dto);
            return CreatedAtAction(nameof(ObterAnotacaoPorId), new { id = anotacao.AnotacaoId }, anotacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar anotação");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma anotação existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AnotacaoGeralResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarAnotacao(int id, [FromBody] CadastroAnotacaoGeralDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var anotacao = await _anotacaoGeralService.AtualizarAnotacaoAsync(id, dto);
            return Ok(anotacao);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar anotação: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui uma anotação
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirAnotacao(int id)
    {
        try
        {
            await _anotacaoGeralService.ExcluirAnotacaoAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir anotação: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
