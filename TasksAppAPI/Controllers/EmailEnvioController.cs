using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/email-envio")]
public class EmailEnvioController : ControllerBase
{
    private readonly IEmailEnvioService _service;
    private readonly IEmailEnvioCampanhaService _campanhaService;
    private readonly ILogger<EmailEnvioController> _logger;

    public EmailEnvioController(
        IEmailEnvioService service,
        IEmailEnvioCampanhaService campanhaService,
        ILogger<EmailEnvioController> logger)
    {
        _service = service;
        _campanhaService = campanhaService;
        _logger = logger;
    }

    [HttpGet("destinatarios")]
    [ProducesResponseType(typeof(DestinatariosEmailPaginadoResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PesquisarDestinatarios(
        [FromQuery] string? termo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 15)
    {
        try
        {
            var resultado = await _service.PesquisarDestinatariosAsync(termo, pagina, tamanhoPagina);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao pesquisar destinatários de e-mail");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPatch("destinatarios/{id}/nao-enviar")]
    [ProducesResponseType(typeof(DestinatarioEmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarNaoEnviar(int id, [FromBody] AlterarNaoEnviarEmailDto dto)
    {
        try
        {
            var atualizado = await _service.AtualizarNaoEnviarAsync(id, dto.NaoEnviar);
            if (atualizado == null)
                return NotFound(new { message = "Destinatário não encontrado" });
            return Ok(atualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar flag não enviar do destinatário {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("enviar")]
    [ProducesResponseType(typeof(EnfileirarCampanhaEmailResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnfileirarEnvio(
        [FromForm] string assunto,
        [FromForm] string corpoHtml,
        [FromForm] List<string> destinatarios,
        [FromForm] List<IFormFile>? anexos)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(assunto))
                return BadRequest(new { message = "Assunto é obrigatório." });
            if (string.IsNullOrWhiteSpace(corpoHtml))
                return BadRequest(new { message = "Corpo do e-mail é obrigatório." });
            if (destinatarios == null || destinatarios.Count == 0)
                return BadRequest(new { message = "Selecione ao menos um destinatário." });

            var resultado = await _campanhaService.EnfileirarCampanhaAsync(
                assunto,
                corpoHtml,
                destinatarios,
                anexos);

            return Accepted(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar campanha de e-mail");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("campanhas/ativa")]
    [ProducesResponseType(typeof(CampanhaEmailStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ObterCampanhaAtiva()
    {
        var status = await _campanhaService.ObterCampanhaAtivaAsync();
        if (status == null)
            return NoContent();
        return Ok(status);
    }

    [HttpGet("campanhas/{id}")]
    [ProducesResponseType(typeof(CampanhaEmailStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterStatusCampanha(int id)
    {
        var status = await _campanhaService.ObterStatusCampanhaAsync(id);
        if (status == null)
            return NotFound(new { message = "Campanha não encontrada." });
        return Ok(status);
    }

    [HttpGet("campanhas/{id}/relatorio")]
    [ProducesResponseType(typeof(RelatorioCampanhaEmailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterRelatorioCampanha(int id)
    {
        var relatorio = await _campanhaService.ObterRelatorioAsync(id);
        if (relatorio == null)
            return NotFound(new { message = "Relatório não disponível. A campanha pode ainda estar em andamento." });
        return Ok(relatorio);
    }

    [HttpGet("campanhas/relatorios")]
    [ProducesResponseType(typeof(IReadOnlyList<RelatorioCampanhaEmailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRelatorios()
    {
        var relatorios = await _campanhaService.ListarRelatoriosAsync();
        return Ok(relatorios);
    }
}
