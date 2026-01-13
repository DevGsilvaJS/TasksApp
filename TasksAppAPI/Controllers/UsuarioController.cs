using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(
        IUsuarioService usuarioService,
        ILogger<UsuarioController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra um novo usuário
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarUsuario([FromBody] CadastroUsuarioDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _usuarioService.CadastrarUsuarioAsync(dto);
            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.UsuarioId }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterUsuarioPorId(int id)
    {
        try
        {
            var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um usuário por login
    /// </summary>
    [HttpGet("login/{login}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterUsuarioPorLogin(string login)
    {
        try
        {
            var usuario = await _usuarioService.ObterUsuarioPorLoginAsync(login);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário por login: {Login}", login);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todos os usuários
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodosUsuarios()
    {
        try
        {
            var usuarios = await _usuarioService.ListarTodosUsuariosAsync();
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
