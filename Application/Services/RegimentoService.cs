using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class RegimentoService : IRegimentoService
{
    private static readonly string[] LoginsObrigatorios = ["TI.GABRIEL", "TI.ABNER"];

    private readonly IRepository<Regimento> _regimentoRepository;
    private readonly IRepository<RegimentoAceite> _aceiteRepository;
    private readonly IRepository<RegimentoAceiteLog> _logRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    public RegimentoService(
        IRepository<Regimento> regimentoRepository,
        IRepository<RegimentoAceite> aceiteRepository,
        IRepository<RegimentoAceiteLog> logRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _regimentoRepository = regimentoRepository;
        _aceiteRepository = aceiteRepository;
        _logRepository = logRepository;
        _usuarioRepository = usuarioRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<RegimentoResponseDto> CadastrarRegimentoAsync(CadastroRegimentoDto dto)
    {
        ValidarCadastro(dto);

        var regimento = new Regimento
        {
            RegTitulo = dto.Titulo.Trim(),
            RegDescricao = dto.Descricao.Trim(),
            RegStatus = dto.Ativo ? (int)StatusRegimento.Ativo : (int)StatusRegimento.Inativo
        };

        await _regimentoRepository.InserirAsync(regimento);
        await _regimentoRepository.SalvarAlteracoesAsync();

        var usuariosObrigatorios = await ObterUsuariosObrigatoriosAsync();
        return MapearResumo(regimento, [], usuariosObrigatorios);
    }

    public async Task<RegimentoDetalheResponseDto?> ObterRegimentoPorIdAsync(int id, int? usuarioId = null)
    {
        var regimento = await _regimentoRepository.GetByIdAsync(id);
        if (regimento == null)
            return null;

        var aceites = (await _aceiteRepository.BuscarTodosAsync(a => a.RegId == id)).ToList();
        var nomesUsuarios = await ObterNomesUsuariosAsync(aceites.Select(a => a.UsuId));
        var usuariosObrigatorios = await ObterUsuariosObrigatoriosAsync();

        return MapearDetalhe(regimento, aceites, nomesUsuarios, usuariosObrigatorios, usuarioId);
    }

    public async Task<IEnumerable<RegimentoResponseDto>> ListarRegimentosAsync()
    {
        var regimentos = (await _regimentoRepository.ListarTodosAsync())
            .OrderByDescending(r => r.RegId)
            .ToList();

        if (regimentos.Count == 0)
            return [];

        var regimentoIds = regimentos.Select(r => r.RegId).ToList();
        var aceites = (await _aceiteRepository.BuscarTodosAsync(a => regimentoIds.Contains(a.RegId))).ToList();
        var usuariosObrigatorios = await ObterUsuariosObrigatoriosAsync();

        return regimentos.Select(r =>
        {
            var aceitesRegimento = aceites.Where(a => a.RegId == r.RegId).ToList();
            return MapearResumo(r, aceitesRegimento, usuariosObrigatorios);
        });
    }

    public async Task<RegimentoResponseDto> AtualizarRegimentoAsync(int id, CadastroRegimentoDto dto)
    {
        ValidarCadastro(dto);

        var regimento = await _regimentoRepository.GetByIdAsync(id);
        if (regimento == null)
            throw new InvalidOperationException("Regimento não encontrado.");

        regimento.RegTitulo = dto.Titulo.Trim();
        regimento.RegDescricao = dto.Descricao.Trim();
        regimento.RegStatus = dto.Ativo ? (int)StatusRegimento.Ativo : (int)StatusRegimento.Inativo;

        await _regimentoRepository.AtualizarAsync(regimento);
        await _regimentoRepository.SalvarAlteracoesAsync();

        var aceites = (await _aceiteRepository.BuscarTodosAsync(a => a.RegId == id)).ToList();
        var usuariosObrigatorios = await ObterUsuariosObrigatoriosAsync();
        return MapearResumo(regimento, aceites, usuariosObrigatorios);
    }

    public async Task ExcluirRegimentoAsync(int id)
    {
        var regimento = await _regimentoRepository.GetByIdAsync(id);
        if (regimento == null)
            throw new InvalidOperationException("Regimento não encontrado.");

        var possuiAceites = await _aceiteRepository.ExisteAsync(a => a.RegId == id);
        if (possuiAceites)
            throw new InvalidOperationException("Não é possível excluir regimento com aceites registrados.");

        await _regimentoRepository.ExcluirAsync(regimento);
        await _regimentoRepository.SalvarAlteracoesAsync();
    }

    public async Task<RegimentoAceiteResponseDto> RegistrarAceiteAsync(int regimentoId, int usuarioId, CadastroRegimentoAceiteDto dto)
    {
        var regimento = await _regimentoRepository.GetByIdAsync(regimentoId);
        if (regimento == null)
            throw new InvalidOperationException("Regimento não encontrado.");

        if (regimento.RegStatus != (int)StatusRegimento.Ativo)
            throw new InvalidOperationException("Não é possível registrar aceite em regimento inativo.");

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var usuariosObrigatorios = await ObterUsuariosObrigatoriosAsync();
        if (!usuariosObrigatorios.Any(u => u.UsuId == usuarioId))
            throw new InvalidOperationException("Somente usuários obrigatórios podem registrar aceite do regimento.");

        var aceiteExistente = await _aceiteRepository.BuscarAsync(a => a.RegId == regimentoId && a.UsuId == usuarioId);
        if (aceiteExistente != null)
            throw new InvalidOperationException("Você já registrou um aceite para este regimento. Desfaça-o antes de registrar novamente.");

        var aceite = new RegimentoAceite
        {
            RegId = regimentoId,
            UsuId = usuarioId,
            RacAceito = dto.Aceito ? (int)AceiteRegimento.Aceito : (int)AceiteRegimento.NaoAceito,
            RacObservacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim(),
            RacDataAceite = DateTime.UtcNow
        };

        await _aceiteRepository.InserirAsync(aceite);
        await _aceiteRepository.SalvarAlteracoesAsync();

        await RegistrarLogAsync(
            regimentoId,
            usuarioId,
            dto.Aceito ? TipoLogRegimentoAceite.Aceite : TipoLogRegimentoAceite.Recusa,
            dto.Aceito ? (int)AceiteRegimento.Aceito : (int)AceiteRegimento.NaoAceito,
            aceite.RacObservacao);

        var nomesUsuarios = await ObterNomesUsuariosAsync([usuarioId]);
        return MapearAceite(aceite, nomesUsuarios);
    }

    public async Task DesfazerAceiteAsync(int aceiteId, int usuarioId)
    {
        var aceite = await _aceiteRepository.GetByIdAsync(aceiteId);
        if (aceite == null)
            throw new InvalidOperationException("Aceite não encontrado.");

        if (aceite.UsuId != usuarioId)
            throw new InvalidOperationException("Somente o usuário que registrou o aceite pode desfazê-lo.");

        await RegistrarLogAsync(
            aceite.RegId,
            usuarioId,
            TipoLogRegimentoAceite.Desfazimento,
            aceite.RacAceito,
            aceite.RacObservacao);

        await _aceiteRepository.ExcluirAsync(aceite);
        await _aceiteRepository.SalvarAlteracoesAsync();
    }

    public async Task<IEnumerable<RegimentoAceiteLogResponseDto>> ListarLogAceitesAsync(int regimentoId)
    {
        var regimento = await _regimentoRepository.GetByIdAsync(regimentoId);
        if (regimento == null)
            throw new InvalidOperationException("Regimento não encontrado.");

        var logs = (await _logRepository.BuscarTodosAsync(l => l.RegId == regimentoId))
            .OrderByDescending(l => l.RalData)
            .ThenByDescending(l => l.RalId)
            .ToList();

        var nomesUsuarios = await ObterNomesUsuariosAsync(logs.Select(l => l.UsuId));

        return logs.Select(l => MapearLog(l, nomesUsuarios));
    }

    private async Task RegistrarLogAsync(
        int regimentoId,
        int usuarioId,
        TipoLogRegimentoAceite tipo,
        int? decisao,
        string? observacao)
    {
        var log = new RegimentoAceiteLog
        {
            RegId = regimentoId,
            UsuId = usuarioId,
            RalTipo = (int)tipo,
            RalDecisao = decisao,
            RalObservacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            RalData = DateTime.UtcNow
        };

        await _logRepository.InserirAsync(log);
        await _logRepository.SalvarAlteracoesAsync();
    }

    private static void ValidarCadastro(CadastroRegimentoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new InvalidOperationException("Título é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            throw new InvalidOperationException("Descrição é obrigatória.");
    }

    private async Task<List<Usuario>> ObterUsuariosObrigatoriosAsync()
    {
        var usuarios = (await _usuarioRepository.BuscarTodosAsync(u =>
            u.UsuLogin != null && LoginsObrigatorios.Contains(u.UsuLogin))).ToList();

        if (usuarios.Count < LoginsObrigatorios.Length)
            throw new InvalidOperationException("Usuários obrigatórios do regimento (Gabriel e Abner) não foram encontrados.");

        return usuarios;
    }

    private async Task<Dictionary<int, string>> ObterNomesUsuariosAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        var usuarios = (await _usuarioRepository.BuscarTodosAsync(u => idList.Contains(u.UsuId))).ToList();
        var pesIds = usuarios.Select(u => u.PesId).Distinct().ToList();
        var pessoas = pesIds.Count == 0
            ? []
            : (await _pessoaRepository.BuscarTodosAsync(p => pesIds.Contains(p.PesId))).ToDictionary(p => p.PesId);

        return usuarios.ToDictionary(
            u => u.UsuId,
            u =>
            {
                if (pessoas.TryGetValue(u.PesId, out var pessoa) && !string.IsNullOrWhiteSpace(pessoa.PesFantasia))
                    return pessoa.PesFantasia;
                return u.UsuLogin ?? $"Usuário {u.UsuId}";
            });
    }

    private static RegimentoResponseDto MapearResumo(
        Regimento regimento,
        IReadOnlyList<RegimentoAceite> aceites,
        IReadOnlyList<Usuario> usuariosObrigatorios)
    {
        var situacao = CalcularSituacaoAprovacao(aceites, usuariosObrigatorios);
        var quantidadeAceites = aceites.Count(a => a.RacAceito == (int)AceiteRegimento.Aceito);

        return new RegimentoResponseDto
        {
            RegimentoId = regimento.RegId,
            Titulo = regimento.RegTitulo,
            Descricao = regimento.RegDescricao,
            Ativo = regimento.RegStatus == (int)StatusRegimento.Ativo,
            SituacaoAprovacao = ObterRotuloSituacao(situacao),
            QuantidadeAceites = quantidadeAceites,
            PossuiAceites = aceites.Count > 0
        };
    }

    private static RegimentoDetalheResponseDto MapearDetalhe(
        Regimento regimento,
        IReadOnlyList<RegimentoAceite> aceites,
        IReadOnlyDictionary<int, string> nomesUsuarios,
        IReadOnlyList<Usuario> usuariosObrigatorios,
        int? usuarioId)
    {
        var resumo = MapearResumo(regimento, aceites, usuariosObrigatorios);
        RegimentoAceiteResponseDto? meuAceiteAtual = null;

        if (usuarioId.HasValue)
        {
            var aceiteUsuario = aceites
                .Where(a => a.UsuId == usuarioId.Value)
                .OrderByDescending(a => a.RacDataAceite ?? DateTime.MinValue)
                .ThenByDescending(a => a.RacId)
                .FirstOrDefault();

            if (aceiteUsuario != null)
                meuAceiteAtual = MapearAceite(aceiteUsuario, nomesUsuarios);
        }

        return new RegimentoDetalheResponseDto
        {
            RegimentoId = resumo.RegimentoId,
            Titulo = resumo.Titulo,
            Descricao = resumo.Descricao,
            Ativo = resumo.Ativo,
            SituacaoAprovacao = resumo.SituacaoAprovacao,
            QuantidadeAceites = resumo.QuantidadeAceites,
            PossuiAceites = resumo.PossuiAceites,
            MeuAceiteAtual = meuAceiteAtual,
            Aceites = aceites
                .OrderByDescending(a => a.RacDataAceite ?? DateTime.MinValue)
                .ThenByDescending(a => a.RacId)
                .Select(a => MapearAceite(a, nomesUsuarios))
        };
    }

    private static RegimentoAceiteResponseDto MapearAceite(RegimentoAceite aceite, IReadOnlyDictionary<int, string> nomesUsuarios)
    {
        var nome = nomesUsuarios.GetValueOrDefault(aceite.UsuId, $"Usuário {aceite.UsuId}");

        return new RegimentoAceiteResponseDto
        {
            AceiteId = aceite.RacId,
            UsuarioId = aceite.UsuId,
            UsuarioNome = nome,
            Aceito = aceite.RacAceito == (int)AceiteRegimento.Aceito,
            Situacao = aceite.RacAceito == (int)AceiteRegimento.Aceito ? "Aceito" : "Recusado",
            Observacao = aceite.RacObservacao,
            DataAceite = aceite.RacDataAceite
        };
    }

    private static RegimentoAceiteLogResponseDto MapearLog(
        RegimentoAceiteLog log,
        IReadOnlyDictionary<int, string> nomesUsuarios)
    {
        var nome = nomesUsuarios.GetValueOrDefault(log.UsuId, $"Usuário {log.UsuId}");

        return new RegimentoAceiteLogResponseDto
        {
            LogId = log.RalId,
            UsuarioId = log.UsuId,
            UsuarioNome = nome,
            Acao = ObterRotuloAcaoLog(log.RalTipo, log.RalDecisao),
            Observacao = log.RalObservacao,
            Data = log.RalData
        };
    }

    private static string ObterRotuloAcaoLog(int tipo, int? decisao) => (TipoLogRegimentoAceite)tipo switch
    {
        TipoLogRegimentoAceite.Aceite => "Aceite",
        TipoLogRegimentoAceite.Recusa => "Recusa",
        TipoLogRegimentoAceite.Desfazimento => decisao == (int)AceiteRegimento.Aceito
            ? "Desfazimento de Aceite"
            : "Desfazimento de Recusa",
        _ => "Ação"
    };

    private static SituacaoAprovacaoRegimento CalcularSituacaoAprovacao(
        IReadOnlyList<RegimentoAceite> aceites,
        IReadOnlyList<Usuario> usuariosObrigatorios)
    {
        var idsObrigatorios = usuariosObrigatorios.Select(u => u.UsuId).ToHashSet();
        var aceitesObrigatorios = aceites
            .Where(a => idsObrigatorios.Contains(a.UsuId))
            .GroupBy(a => a.UsuId)
            .Select(g => g.OrderByDescending(a => a.RacDataAceite ?? DateTime.MinValue)
                .ThenByDescending(a => a.RacId)
                .First())
            .ToList();

        var quantidadeAceitos = aceitesObrigatorios.Count(a => a.RacAceito == (int)AceiteRegimento.Aceito);

        return quantidadeAceitos switch
        {
            2 => SituacaoAprovacaoRegimento.Aprovado,
            1 => SituacaoAprovacaoRegimento.ParcialmenteAprovado,
            _ => SituacaoAprovacaoRegimento.Reprovado
        };
    }

    private static string ObterRotuloSituacao(SituacaoAprovacaoRegimento situacao) => situacao switch
    {
        SituacaoAprovacaoRegimento.Aprovado => "Aprovado",
        SituacaoAprovacaoRegimento.ParcialmenteAprovado => "Parcialmente Aprovado",
        _ => "Reprovado"
    };
}
