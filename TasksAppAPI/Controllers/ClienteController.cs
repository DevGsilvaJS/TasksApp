using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(
        IClienteService clienteService,
        ILogger<ClienteController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra um novo cliente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarCliente([FromBody] CadastroClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _clienteService.CadastrarClienteAsync(dto);
            return CreatedAtAction(nameof(ObterClientePorId), new { id = cliente.ClienteId }, cliente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar cliente");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterClientePorId(int id)
    {
        try
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cliente por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodosClientes()
    {
        try
        {
            var clientes = await _clienteService.ListarTodosClientesAsync();
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar clientes");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um cliente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarCliente(int id, [FromBody] CadastroClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _clienteService.AtualizarClienteAsync(id, dto);
            return Ok(cliente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui um cliente
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirCliente(int id)
    {
        try
        {
            await _clienteService.ExcluirClienteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
