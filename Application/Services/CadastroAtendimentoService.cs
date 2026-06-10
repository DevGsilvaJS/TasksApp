using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CadastroAtendimentoService : ICadastroAtendimentoService
{
    private readonly IRepository<CadastroStatusTarefa> _statusRepo;
    private readonly IRepository<CadastroTipoAtendimento> _tipoAtendimentoRepo;
    private readonly IRepository<CadastroTipoContato> _tipoContatoRepo;
    private readonly IRepository<CadastroAndamento> _andamentoRepo;
    private readonly IRepository<CadastroStatusAtendimentoComercial> _statusComercialRepo;
    private readonly IRepository<PossivelCliente> _possivelClienteRepo;
    private readonly IRepository<Tarefa> _tarefaRepo;

    public CadastroAtendimentoService(
        IRepository<CadastroStatusTarefa> statusRepo,
        IRepository<CadastroTipoAtendimento> tipoAtendimentoRepo,
        IRepository<CadastroTipoContato> tipoContatoRepo,
        IRepository<CadastroAndamento> andamentoRepo,
        IRepository<CadastroStatusAtendimentoComercial> statusComercialRepo,
        IRepository<PossivelCliente> possivelClienteRepo,
        IRepository<Tarefa> tarefaRepo)
    {
        _statusRepo = statusRepo;
        _tipoAtendimentoRepo = tipoAtendimentoRepo;
        _tipoContatoRepo = tipoContatoRepo;
        _andamentoRepo = andamentoRepo;
        _statusComercialRepo = statusComercialRepo;
        _possivelClienteRepo = possivelClienteRepo;
        _tarefaRepo = tarefaRepo;
    }

    private static CadastroStatusTarefaResponseDto ToStatusDto(CadastroStatusTarefa e) =>
        new() { Id = e.Id, Descricao = e.Descricao, Ativo = e.Ativo };
    private static CadastroTipoAtendimentoResponseDto ToTipoAtendimentoDto(CadastroTipoAtendimento e) =>
        new() { Id = e.Id, Descricao = e.Descricao, Ativo = e.Ativo };
    private static CadastroTipoContatoResponseDto ToTipoContatoDto(CadastroTipoContato e) =>
        new() { Id = e.Id, Descricao = e.Descricao, Ativo = e.Ativo };
    private static CadastroAndamentoResponseDto ToAndamentoDto(CadastroAndamento e) =>
        new() { Id = e.Id, Descricao = e.Descricao, Ativo = e.Ativo };
    private static CadastroStatusAtendimentoComercialResponseDto ToStatusComercialDto(CadastroStatusAtendimentoComercial e) =>
        new() { Id = e.Id, Numero = e.Numero, Descricao = e.Descricao, Ativo = e.Ativo };

    public async Task<IEnumerable<CadastroStatusTarefaResponseDto>> ListarStatusTarefaAsync(bool? apenasAtivos = null)
    {
        var list = apenasAtivos == true
            ? await _statusRepo.BuscarTodosAsync(x => x.Ativo)
            : await _statusRepo.ListarTodosAsync();
        return list.OrderBy(x => x.Id).Select(ToStatusDto);
    }

    public async Task<CadastroStatusTarefaResponseDto?> ObterStatusTarefaPorIdAsync(int id)
    {
        var e = await _statusRepo.GetByIdAsync(id);
        return e == null ? null : ToStatusDto(e);
    }

    public async Task<CadastroStatusTarefaResponseDto> CriarStatusTarefaAsync(CadastroStatusTarefaRequestDto dto)
    {
        var todos = await _statusRepo.ListarTodosAsync();
        var nextId = todos.Any() ? todos.Max(x => x.Id) + 1 : 1;
        var entity = new CadastroStatusTarefa { Id = nextId, Descricao = dto.Descricao.Trim(), Ativo = dto.Ativo };
        await _statusRepo.InserirAsync(entity);
        await _statusRepo.SalvarAlteracoesAsync();
        return ToStatusDto(entity);
    }

    public async Task<CadastroStatusTarefaResponseDto?> AtualizarStatusTarefaAsync(int id, CadastroStatusTarefaRequestDto dto)
    {
        var e = await _statusRepo.GetByIdAsync(id);
        if (e == null) return null;
        e.Descricao = dto.Descricao.Trim();
        e.Ativo = dto.Ativo;
        await _statusRepo.AtualizarAsync(e);
        await _statusRepo.SalvarAlteracoesAsync();
        return ToStatusDto(e);
    }

    public async Task<bool> AlterarAtivoStatusTarefaAsync(int id, bool ativo)
    {
        var e = await _statusRepo.GetByIdAsync(id);
        if (e == null) return false;
        e.Ativo = ativo;
        await _statusRepo.AtualizarAsync(e);
        await _statusRepo.SalvarAlteracoesAsync();
        return true;
    }

    public async Task<IEnumerable<CadastroTipoAtendimentoResponseDto>> ListarTipoAtendimentoAsync(bool? apenasAtivos = null)
    {
        var list = apenasAtivos == true
            ? await _tipoAtendimentoRepo.BuscarTodosAsync(x => x.Ativo)
            : await _tipoAtendimentoRepo.ListarTodosAsync();
        return list.OrderBy(x => x.Id).Select(ToTipoAtendimentoDto);
    }

    public async Task<CadastroTipoAtendimentoResponseDto?> ObterTipoAtendimentoPorIdAsync(int id)
    {
        var e = await _tipoAtendimentoRepo.GetByIdAsync(id);
        return e == null ? null : ToTipoAtendimentoDto(e);
    }

    public async Task<CadastroTipoAtendimentoResponseDto> CriarTipoAtendimentoAsync(CadastroTipoAtendimentoRequestDto dto)
    {
        var todos = await _tipoAtendimentoRepo.ListarTodosAsync();
        var nextId = todos.Any() ? todos.Max(x => x.Id) + 1 : 1;
        var entity = new CadastroTipoAtendimento { Id = nextId, Descricao = dto.Descricao.Trim(), Ativo = dto.Ativo };
        await _tipoAtendimentoRepo.InserirAsync(entity);
        await _tipoAtendimentoRepo.SalvarAlteracoesAsync();
        return ToTipoAtendimentoDto(entity);
    }

    public async Task<CadastroTipoAtendimentoResponseDto?> AtualizarTipoAtendimentoAsync(int id, CadastroTipoAtendimentoRequestDto dto)
    {
        var e = await _tipoAtendimentoRepo.GetByIdAsync(id);
        if (e == null) return null;
        e.Descricao = dto.Descricao.Trim();
        e.Ativo = dto.Ativo;
        await _tipoAtendimentoRepo.AtualizarAsync(e);
        await _tipoAtendimentoRepo.SalvarAlteracoesAsync();
        return ToTipoAtendimentoDto(e);
    }

    public async Task<bool> AlterarAtivoTipoAtendimentoAsync(int id, bool ativo)
    {
        var e = await _tipoAtendimentoRepo.GetByIdAsync(id);
        if (e == null) return false;
        e.Ativo = ativo;
        await _tipoAtendimentoRepo.AtualizarAsync(e);
        await _tipoAtendimentoRepo.SalvarAlteracoesAsync();
        return true;
    }

    public async Task ExcluirTipoAtendimentoAsync(int id)
    {
        var e = await _tipoAtendimentoRepo.GetByIdAsync(id);
        if (e == null)
            throw new KeyNotFoundException("Tipo de atendimento não encontrado.");
        if (id <= (int)TipoAtendimento.Cobranca)
            throw new InvalidOperationException("Não é possível excluir tipos de atendimento padrão do sistema.");
        var tipo = (TipoAtendimento)id;
        var emUso = await _tarefaRepo.BuscarTodosAsync(t => t.TarTipoAtendimento == tipo);
        if (emUso.Any())
            throw new InvalidOperationException("Não é possível excluir: existem tarefas usando este tipo de atendimento.");
        await _tipoAtendimentoRepo.ExcluirAsync(e);
        await _tipoAtendimentoRepo.SalvarAlteracoesAsync();
    }

    public async Task<IEnumerable<CadastroTipoContatoResponseDto>> ListarTipoContatoAsync(bool? apenasAtivos = null)
    {
        var list = apenasAtivos == true
            ? await _tipoContatoRepo.BuscarTodosAsync(x => x.Ativo)
            : await _tipoContatoRepo.ListarTodosAsync();
        return list.OrderBy(x => x.Id).Select(ToTipoContatoDto);
    }

    public async Task<CadastroTipoContatoResponseDto?> ObterTipoContatoPorIdAsync(int id)
    {
        var e = await _tipoContatoRepo.GetByIdAsync(id);
        return e == null ? null : ToTipoContatoDto(e);
    }

    public async Task<CadastroTipoContatoResponseDto> CriarTipoContatoAsync(CadastroTipoContatoRequestDto dto)
    {
        var todos = await _tipoContatoRepo.ListarTodosAsync();
        var nextId = todos.Any() ? todos.Max(x => x.Id) + 1 : 1;
        var entity = new CadastroTipoContato { Id = nextId, Descricao = dto.Descricao.Trim(), Ativo = dto.Ativo };
        await _tipoContatoRepo.InserirAsync(entity);
        await _tipoContatoRepo.SalvarAlteracoesAsync();
        return ToTipoContatoDto(entity);
    }

    public async Task<CadastroTipoContatoResponseDto?> AtualizarTipoContatoAsync(int id, CadastroTipoContatoRequestDto dto)
    {
        var e = await _tipoContatoRepo.GetByIdAsync(id);
        if (e == null) return null;
        e.Descricao = dto.Descricao.Trim();
        e.Ativo = dto.Ativo;
        await _tipoContatoRepo.AtualizarAsync(e);
        await _tipoContatoRepo.SalvarAlteracoesAsync();
        return ToTipoContatoDto(e);
    }

    public async Task<bool> AlterarAtivoTipoContatoAsync(int id, bool ativo)
    {
        var e = await _tipoContatoRepo.GetByIdAsync(id);
        if (e == null) return false;
        e.Ativo = ativo;
        await _tipoContatoRepo.AtualizarAsync(e);
        await _tipoContatoRepo.SalvarAlteracoesAsync();
        return true;
    }

    public async Task<IEnumerable<CadastroAndamentoResponseDto>> ListarAndamentoAsync(bool? apenasAtivos = null)
    {
        var list = apenasAtivos == true
            ? await _andamentoRepo.BuscarTodosAsync(x => x.Ativo)
            : await _andamentoRepo.ListarTodosAsync();
        return list.OrderBy(x => x.Id).Select(ToAndamentoDto);
    }

    public async Task<CadastroAndamentoResponseDto?> ObterAndamentoPorIdAsync(int id)
    {
        var e = await _andamentoRepo.GetByIdAsync(id);
        return e == null ? null : ToAndamentoDto(e);
    }

    public async Task<CadastroAndamentoResponseDto> CriarAndamentoAsync(CadastroAndamentoRequestDto dto)
    {
        var todos = await _andamentoRepo.ListarTodosAsync();
        var nextId = todos.Any() ? todos.Max(x => x.Id) + 1 : 1;
        var entity = new CadastroAndamento { Id = nextId, Descricao = dto.Descricao.Trim(), Ativo = dto.Ativo };
        await _andamentoRepo.InserirAsync(entity);
        await _andamentoRepo.SalvarAlteracoesAsync();
        return ToAndamentoDto(entity);
    }

    public async Task<CadastroAndamentoResponseDto?> AtualizarAndamentoAsync(int id, CadastroAndamentoRequestDto dto)
    {
        var e = await _andamentoRepo.GetByIdAsync(id);
        if (e == null) return null;
        e.Descricao = dto.Descricao.Trim();
        e.Ativo = dto.Ativo;
        await _andamentoRepo.AtualizarAsync(e);
        await _andamentoRepo.SalvarAlteracoesAsync();
        return ToAndamentoDto(e);
    }

    public async Task<bool> AlterarAtivoAndamentoAsync(int id, bool ativo)
    {
        var e = await _andamentoRepo.GetByIdAsync(id);
        if (e == null) return false;
        e.Ativo = ativo;
        await _andamentoRepo.AtualizarAsync(e);
        await _andamentoRepo.SalvarAlteracoesAsync();
        return true;
    }

    public async Task<IEnumerable<CadastroStatusAtendimentoComercialResponseDto>> ListarStatusAtendimentoComercialAsync(bool? apenasAtivos = null)
    {
        var list = apenasAtivos == true
            ? await _statusComercialRepo.BuscarTodosAsync(x => x.Ativo)
            : await _statusComercialRepo.ListarTodosAsync();
        return list.OrderBy(x => x.Numero).Select(ToStatusComercialDto);
    }

    public async Task<CadastroStatusAtendimentoComercialResponseDto?> ObterStatusAtendimentoComercialPorIdAsync(int id)
    {
        var e = await _statusComercialRepo.GetByIdAsync(id);
        return e == null ? null : ToStatusComercialDto(e);
    }

    public async Task<CadastroStatusAtendimentoComercialResponseDto> CriarStatusAtendimentoComercialAsync(CadastroStatusAtendimentoComercialRequestDto dto)
    {
        var todos = await _statusComercialRepo.ListarTodosAsync();
        var nextNumero = todos.Any() ? todos.Max(x => x.Numero) + 1 : 1;
        var entity = new CadastroStatusAtendimentoComercial
        {
            Numero = nextNumero,
            Descricao = dto.Descricao.Trim(),
            Ativo = dto.Ativo
        };
        await _statusComercialRepo.InserirAsync(entity);
        await _statusComercialRepo.SalvarAlteracoesAsync();
        return ToStatusComercialDto(entity);
    }

    public async Task<CadastroStatusAtendimentoComercialResponseDto?> AtualizarStatusAtendimentoComercialAsync(int id, CadastroStatusAtendimentoComercialRequestDto dto)
    {
        var e = await _statusComercialRepo.GetByIdAsync(id);
        if (e == null) return null;
        e.Descricao = dto.Descricao.Trim();
        e.Ativo = dto.Ativo;
        await _statusComercialRepo.AtualizarAsync(e);
        await _statusComercialRepo.SalvarAlteracoesAsync();
        return ToStatusComercialDto(e);
    }

    public async Task<bool> AlterarAtivoStatusAtendimentoComercialAsync(int id, bool ativo)
    {
        var e = await _statusComercialRepo.GetByIdAsync(id);
        if (e == null) return false;
        e.Ativo = ativo;
        await _statusComercialRepo.AtualizarAsync(e);
        await _statusComercialRepo.SalvarAlteracoesAsync();
        return true;
    }

    public async Task ExcluirStatusAtendimentoComercialAsync(int id)
    {
        var e = await _statusComercialRepo.GetByIdAsync(id);
        if (e == null)
            throw new KeyNotFoundException("Status de atendimento comercial não encontrado.");
        var emUso = await _possivelClienteRepo.BuscarTodosAsync(p => p.PocStatusAtendimento == e.Numero);
        if (emUso.Any())
            throw new InvalidOperationException("Não é possível excluir: existem possíveis clientes usando este status.");
        await _statusComercialRepo.ExcluirAsync(e);
        await _statusComercialRepo.SalvarAlteracoesAsync();
    }
}
