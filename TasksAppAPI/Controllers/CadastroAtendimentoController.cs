using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

public class AlterarAtivoDto
{
    public bool Ativo { get; set; }
}

[ApiController]
[Route("api/cadastro-atendimento")]
public class CadastroAtendimentoController : ControllerBase
{
    private readonly ICadastroAtendimentoService _service;

    public CadastroAtendimentoController(ICadastroAtendimentoService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<ActionResult<IEnumerable<CadastroStatusTarefaResponseDto>>> ListarStatus([FromQuery] bool? apenasAtivos)
    {
        var list = await _service.ListarStatusTarefaAsync(apenasAtivos);
        return Ok(list);
    }

    [HttpGet("status/{id}")]
    public async Task<ActionResult<CadastroStatusTarefaResponseDto>> ObterStatus(int id)
    {
        var item = await _service.ObterStatusTarefaPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("status")]
    public async Task<ActionResult<CadastroStatusTarefaResponseDto>> CriarStatus([FromBody] CadastroStatusTarefaRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var created = await _service.CriarStatusTarefaAsync(dto);
        return CreatedAtAction(nameof(ObterStatus), new { id = created.Id }, created);
    }

    [HttpPut("status/{id}")]
    public async Task<ActionResult<CadastroStatusTarefaResponseDto>> AtualizarStatus(int id, [FromBody] CadastroStatusTarefaRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var updated = await _service.AtualizarStatusTarefaAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPatch("status/{id}/ativo")]
    public async Task<ActionResult> AlterarAtivoStatus(int id, [FromBody] AlterarAtivoDto dto)
    {
        var ok = await _service.AlterarAtivoStatusTarefaAsync(id, dto.Ativo);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpGet("tipo-atendimento")]
    public async Task<ActionResult<IEnumerable<CadastroTipoAtendimentoResponseDto>>> ListarTipoAtendimento([FromQuery] bool? apenasAtivos)
    {
        var list = await _service.ListarTipoAtendimentoAsync(apenasAtivos);
        return Ok(list);
    }

    [HttpGet("tipo-atendimento/{id}")]
    public async Task<ActionResult<CadastroTipoAtendimentoResponseDto>> ObterTipoAtendimento(int id)
    {
        var item = await _service.ObterTipoAtendimentoPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("tipo-atendimento")]
    public async Task<ActionResult<CadastroTipoAtendimentoResponseDto>> CriarTipoAtendimento([FromBody] CadastroTipoAtendimentoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var created = await _service.CriarTipoAtendimentoAsync(dto);
        return CreatedAtAction(nameof(ObterTipoAtendimento), new { id = created.Id }, created);
    }

    [HttpPut("tipo-atendimento/{id}")]
    public async Task<ActionResult<CadastroTipoAtendimentoResponseDto>> AtualizarTipoAtendimento(int id, [FromBody] CadastroTipoAtendimentoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var updated = await _service.AtualizarTipoAtendimentoAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPatch("tipo-atendimento/{id}/ativo")]
    public async Task<ActionResult> AlterarAtivoTipoAtendimento(int id, [FromBody] AlterarAtivoDto dto)
    {
        var ok = await _service.AlterarAtivoTipoAtendimentoAsync(id, dto.Ativo);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpGet("tipo-contato")]
    public async Task<ActionResult<IEnumerable<CadastroTipoContatoResponseDto>>> ListarTipoContato([FromQuery] bool? apenasAtivos)
    {
        var list = await _service.ListarTipoContatoAsync(apenasAtivos);
        return Ok(list);
    }

    [HttpGet("tipo-contato/{id}")]
    public async Task<ActionResult<CadastroTipoContatoResponseDto>> ObterTipoContato(int id)
    {
        var item = await _service.ObterTipoContatoPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("tipo-contato")]
    public async Task<ActionResult<CadastroTipoContatoResponseDto>> CriarTipoContato([FromBody] CadastroTipoContatoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var created = await _service.CriarTipoContatoAsync(dto);
        return CreatedAtAction(nameof(ObterTipoContato), new { id = created.Id }, created);
    }

    [HttpPut("tipo-contato/{id}")]
    public async Task<ActionResult<CadastroTipoContatoResponseDto>> AtualizarTipoContato(int id, [FromBody] CadastroTipoContatoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var updated = await _service.AtualizarTipoContatoAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPatch("tipo-contato/{id}/ativo")]
    public async Task<ActionResult> AlterarAtivoTipoContato(int id, [FromBody] AlterarAtivoDto dto)
    {
        var ok = await _service.AlterarAtivoTipoContatoAsync(id, dto.Ativo);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpGet("status-atendimento-comercial")]
    public async Task<ActionResult<IEnumerable<CadastroStatusAtendimentoComercialResponseDto>>> ListarStatusAtendimentoComercial([FromQuery] bool? apenasAtivos)
    {
        var list = await _service.ListarStatusAtendimentoComercialAsync(apenasAtivos);
        return Ok(list);
    }

    [HttpGet("status-atendimento-comercial/{id}")]
    public async Task<ActionResult<CadastroStatusAtendimentoComercialResponseDto>> ObterStatusAtendimentoComercial(int id)
    {
        var item = await _service.ObterStatusAtendimentoComercialPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("status-atendimento-comercial")]
    public async Task<ActionResult<CadastroStatusAtendimentoComercialResponseDto>> CriarStatusAtendimentoComercial([FromBody] CadastroStatusAtendimentoComercialRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var created = await _service.CriarStatusAtendimentoComercialAsync(dto);
        return CreatedAtAction(nameof(ObterStatusAtendimentoComercial), new { id = created.Id }, created);
    }

    [HttpPut("status-atendimento-comercial/{id}")]
    public async Task<ActionResult<CadastroStatusAtendimentoComercialResponseDto>> AtualizarStatusAtendimentoComercial(int id, [FromBody] CadastroStatusAtendimentoComercialRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return BadRequest(new { message = "Descrição é obrigatória." });
        var updated = await _service.AtualizarStatusAtendimentoComercialAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPatch("status-atendimento-comercial/{id}/ativo")]
    public async Task<ActionResult> AlterarAtivoStatusAtendimentoComercial(int id, [FromBody] AlterarAtivoDto dto)
    {
        var ok = await _service.AlterarAtivoStatusAtendimentoComercialAsync(id, dto.Ativo);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("status-atendimento-comercial/{id}")]
    public async Task<ActionResult> ExcluirStatusAtendimentoComercial(int id)
    {
        try
        {
            await _service.ExcluirStatusAtendimentoComercialAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
