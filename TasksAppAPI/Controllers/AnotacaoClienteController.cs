using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnotacaoClienteController : ControllerBase
{
    private readonly IAnotacaoClienteService _service;
    private readonly ILogger<AnotacaoClienteController> _logger;

    public AnotacaoClienteController(
        IAnotacaoClienteService service,
        ILogger<AnotacaoClienteController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma anotação de e-mail do cliente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AnotacaoClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroAnotacaoClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _service.CadastrarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.AnotacaoClienteId }, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar anotação do cliente");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma anotação por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnotacaoClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var resultado = await _service.ObterPorIdAsync(id);
            if (resultado == null)
                return NotFound(new { message = "Anotação não encontrada" });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter anotação: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista anotações por cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    [ProducesResponseType(typeof(IEnumerable<AnotacaoClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorCliente(int clienteId)
    {
        try
        {
            var resultado = await _service.ListarPorClienteAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar anotações do cliente: {ClienteId}", clienteId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as anotações de clientes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnotacaoClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodas()
    {
        try
        {
            var resultado = await _service.ListarTodasAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar anotações de clientes");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma anotação
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AnotacaoClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CadastroAnotacaoClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _service.AtualizarAsync(id, dto);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _service.ExcluirAsync(id);
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
