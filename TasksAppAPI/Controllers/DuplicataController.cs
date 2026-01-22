using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DuplicataController : ControllerBase
{
    private readonly IDuplicataService _duplicataService;
    private readonly ILogger<DuplicataController> _logger;

    public DuplicataController(
        IDuplicataService duplicataService,
        ILogger<DuplicataController> logger)
    {
        _duplicataService = duplicataService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma nova duplicata
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DuplicataResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarDuplicata([FromBody] CadastroDuplicataDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var duplicata = await _duplicataService.CadastrarDuplicataAsync(dto);
            return CreatedAtAction(nameof(ObterDuplicataPorId), new { id = duplicata.DuplicataId }, duplicata);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar duplicata");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém uma duplicata por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DuplicataResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDuplicataPorId(int id)
    {
        try
        {
            var duplicata = await _duplicataService.ObterDuplicataPorIdAsync(id);
            if (duplicata == null)
            {
                return NotFound(new { message = "Duplicata não encontrada" });
            }

            return Ok(duplicata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter duplicata por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as duplicatas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DuplicataResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodasDuplicatas()
    {
        try
        {
            var duplicatas = await _duplicataService.ListarTodasDuplicatasAsync();
            return Ok(duplicatas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar duplicatas");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista duplicatas por tipo (CP = Contas a Pagar, CR = Contas a Receber)
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(IEnumerable<DuplicataResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDuplicatasPorTipo(string tipo)
    {
        try
        {
            if (tipo != "CP" && tipo != "CR")
            {
                return BadRequest(new { message = "Tipo inválido. Use 'CP' para Contas a Pagar ou 'CR' para Contas a Receber." });
            }

            var duplicatas = await _duplicataService.ListarDuplicatasPorTipoAsync(tipo);
            return Ok(duplicatas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar duplicatas por tipo: {Tipo}", tipo);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém o próximo número disponível para um tipo de duplicata
    /// </summary>
    [HttpGet("proximo-numero/{tipo}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterProximoNumero(string tipo)
    {
        try
        {
            if (tipo != "CP" && tipo != "CR")
            {
                return BadRequest(new { message = "Tipo inválido. Use 'CP' para Contas a Pagar ou 'CR' para Contas a Receber." });
            }

            var proximoNumero = await _duplicataService.ObterProximoNumeroAsync(tipo);
            return Ok(proximoNumero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter próximo número para tipo: {Tipo}", tipo);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma duplicata
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DuplicataResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarDuplicata(int id, [FromBody] CadastroDuplicataDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var duplicata = await _duplicataService.AtualizarDuplicataAsync(id, dto);
            return Ok(duplicata);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar duplicata: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui uma duplicata
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirDuplicata(int id)
    {
        try
        {
            await _duplicataService.ExcluirDuplicataAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir duplicata: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Baixa uma parcela (marca como paga)
    /// </summary>
    [HttpPost("parcelas/{parcelaId}/baixar")]
    [ProducesResponseType(typeof(ParcelaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaixarParcela(int parcelaId)
    {
        try
        {
            var parcela = await _duplicataService.BaixarParcelaAsync(parcelaId);
            return Ok(parcela);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar parcela: {ParcelaId}", parcelaId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Reativa uma parcela paga (marca como pendente)
    /// </summary>
    [HttpPost("parcelas/{parcelaId}/reativar")]
    [ProducesResponseType(typeof(ParcelaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReativarParcela(int parcelaId)
    {
        try
        {
            var parcela = await _duplicataService.ReativarParcelaAsync(parcelaId);
            return Ok(parcela);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reativar parcela: {ParcelaId}", parcelaId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
