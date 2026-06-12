using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly ILogger<EmpresaController> _logger;

    public EmpresaController(IEmpresaService empresaService, ILogger<EmpresaController> logger)
    {
        _empresaService = empresaService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmpresaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarEmpresa([FromBody] CadastroEmpresaDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var empresa = await _empresaService.CadastrarEmpresaAsync(dto);
            return CreatedAtAction(nameof(ObterEmpresaPorId), new { id = empresa.EmpresaId }, empresa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar empresa");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmpresaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterEmpresaPorId(int id)
    {
        try
        {
            var empresa = await _empresaService.ObterEmpresaPorIdAsync(id);
            if (empresa == null) return NotFound(new { message = "Empresa não encontrada" });
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter empresa por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmpresaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodasEmpresas()
    {
        try
        {
            var empresas = await _empresaService.ListarTodasEmpresasAsync();
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar empresas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmpresaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarEmpresa(int id, [FromBody] CadastroEmpresaDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var empresa = await _empresaService.AtualizarEmpresaAsync(id, dto);
            return Ok(empresa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirEmpresa(int id)
    {
        try
        {
            await _empresaService.ExcluirEmpresaAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir empresa {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
