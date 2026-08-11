using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TarefaService : ITarefaService
{
    private const int StatusConcluidaId = (int)StatusTarefa.Concluida;

    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<AnotacaoTarefa> _anotacaoRepository;
    private readonly IRepository<ImagemTarefa> _imagemRepository;
    private readonly IRepository<CadastroStatusTarefa> _cadastroStatusTarefaRepository;
    private readonly IRepository<CadastroTipoAtendimento> _cadastroTipoAtendimentoRepository;
    private readonly IRepository<CadastroTipoContato> _cadastroTipoContatoRepository;

    public TarefaService(
        IRepository<Tarefa> tarefaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<AnotacaoTarefa> anotacaoRepository,
        IRepository<ImagemTarefa> imagemRepository,
        IRepository<CadastroStatusTarefa> cadastroStatusTarefaRepository,
        IRepository<CadastroTipoAtendimento> cadastroTipoAtendimentoRepository,
        IRepository<CadastroTipoContato> cadastroTipoContatoRepository)
    {
        _tarefaRepository = tarefaRepository;
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _pessoaRepository = pessoaRepository;
        _anotacaoRepository = anotacaoRepository;
        _imagemRepository = imagemRepository;
        _cadastroStatusTarefaRepository = cadastroStatusTarefaRepository;
        _cadastroTipoAtendimentoRepository = cadastroTipoAtendimentoRepository;
        _cadastroTipoContatoRepository = cadastroTipoContatoRepository;
    }

    public async Task<TarefaResponseDto> CadastrarTarefaAsync(CadastroTarefaDto dto)
    {
        // Validar se cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        // Validar se usuário existe
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        // Gerar número de atendimento auto incremental
        var todasTarefas = await _tarefaRepository.ListarTodosAsync();
        var proximoNumero = 1;
        if (todasTarefas.Any())
        {
            var maiorNumero = todasTarefas
                .Where(t => t.TarNumero.HasValue)
                .Select(t => t.TarNumero.Value)
                .DefaultIfEmpty(0)
                .Max();
            proximoNumero = maiorNumero + 1;
        }

        // Criar Tarefa
        var tarefa = new Tarefa
        {
            CliId = dto.ClienteId,
            UsuId = dto.UsuarioId,
            TarDtCadastro = dto.DataCadastro.HasValue
                ? DateTime.SpecifyKind(dto.DataCadastro.Value.Date, DateTimeKind.Utc)
                : DateTime.UtcNow,
            TarDtConclusao = dto.DataConclusao?.ToUniversalTime(),
            TarStatus = dto.Status,
            TarTitulo = dto.Titulo?.ToUpper(),
            TarProtocolo = dto.Protocolo?.ToUpper(),
            TarSolicitante = dto.Solicitante?.ToUpper(),
            TarCelularSolicitante = dto.CelularSolicitante,
            TarTipoAtendimento = dto.TipoAtendimento.HasValue ? (TipoAtendimento?)dto.TipoAtendimento.Value : null,
            TarPrioridade = dto.Prioridade,
            TarNumero = proximoNumero,
            TarTipoContato = dto.TipoContato.HasValue ? (TipoContato?)dto.TipoContato.Value : null
        };

        await _tarefaRepository.InserirAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        // Se houver descrição, criar anotação
        if (!string.IsNullOrWhiteSpace(dto.Descricao))
        {
            var anotacao = new AnotacaoTarefa
            {
                TarId = tarefa.TarId,
                UsuId = dto.UsuarioId,
                AntDescricao = dto.Descricao.ToUpper(),
                AntDtCadastro = DateTime.UtcNow
            };

            await _anotacaoRepository.InserirAsync(anotacao);
            await _anotacaoRepository.SalvarAlteracoesAsync();
        }

        return await MontarTarefaResponseDto(tarefa);
    }

    public async Task<TarefaResponseDto?> ObterTarefaPorIdAsync(int id)
    {
        var tarefa = await _tarefaRepository.GetByIdAsync(id);
        if (tarefa == null)
            return null;

        return await MontarTarefaResponseDto(tarefa);
    }

    public async Task<IEnumerable<TarefaResponseDto>> ListarTodasTarefasAsync()
    {
        return await ListarTarefasAsync(usuarioId: null, incluirConcluidas: true);
    }

    public async Task<IEnumerable<TarefaResponseDto>> ListarTarefasAsync(
        int? usuarioId,
        bool incluirConcluidas,
        string? criterio = null,
        string? valor = null)
    {
        var termo = NormalizarTermoPesquisa(valor);
        var criterioNorm = (criterio ?? string.Empty).Trim().ToLowerInvariant();
        var temPesquisa = !string.IsNullOrWhiteSpace(termo) && !string.IsNullOrWhiteSpace(criterioNorm);

        IEnumerable<Tarefa> tarefas;

        if (!temPesquisa)
        {
            if (usuarioId.HasValue && !incluirConcluidas)
            {
                tarefas = await _tarefaRepository.BuscarTodosAsync(t =>
                    t.UsuId == usuarioId.Value && t.TarStatus != StatusConcluidaId);
            }
            else if (usuarioId.HasValue)
            {
                tarefas = await _tarefaRepository.BuscarTodosAsync(t => t.UsuId == usuarioId.Value);
            }
            else if (!incluirConcluidas)
            {
                tarefas = await _tarefaRepository.BuscarTodosAsync(t => t.TarStatus != StatusConcluidaId);
            }
            else
            {
                tarefas = await _tarefaRepository.ListarTodosAsync();
            }
        }
        else
        {
            // Pesquisa por executor ignora filtro de usuário logado (o critério já é o executor)
            var usuarioFiltro = criterioNorm == "executor" ? null : usuarioId;
            tarefas = await BuscarPorCriterioAsync(criterioNorm, termo, usuarioFiltro);

            // Em pesquisa por status, o próprio critério define o status (não aplica exclusão de concluídas)
            if (!incluirConcluidas && criterioNorm != "status")
            {
                tarefas = tarefas.Where(t => t.TarStatus != StatusConcluidaId);
            }
        }

        var resultado = new List<TarefaResponseDto>();
        foreach (var tarefa in tarefas.OrderByDescending(t => t.TarNumero ?? t.TarId))
        {
            resultado.Add(await MontarTarefaResponseDto(tarefa));
        }
        return resultado;
    }

    private async Task<IEnumerable<Tarefa>> BuscarPorCriterioAsync(string criterio, string termo, int? usuarioId)
    {
        switch (criterio)
        {
            case "titulo":
            {
                var upper = termo.ToUpperInvariant();
                if (usuarioId.HasValue)
                {
                    return await _tarefaRepository.BuscarTodosAsync(t =>
                        t.UsuId == usuarioId.Value &&
                        t.TarTitulo != null &&
                        t.TarTitulo.Contains(upper));
                }
                return await _tarefaRepository.BuscarTodosAsync(t =>
                    t.TarTitulo != null &&
                    t.TarTitulo.Contains(upper));
            }
            case "cliente":
            {
                var upper = termo.ToUpperInvariant();
                var pessoas = await _pessoaRepository.BuscarTodosAsync(p =>
                    p.PesFantasia != null && p.PesFantasia.ToUpper().Contains(upper));
                var pesIds = pessoas.Select(p => p.PesId).ToHashSet();

                var clientesPorNome = await _clienteRepository.BuscarTodosAsync(c => pesIds.Contains(c.PesId));
                var clientesPorCodigo = await _clienteRepository.BuscarTodosAsync(c =>
                    c.CliCodigo != null && c.CliCodigo.ToUpper().Contains(upper));

                var cliIds = clientesPorNome.Select(c => c.CliId)
                    .Concat(clientesPorCodigo.Select(c => c.CliId))
                    .ToHashSet();

                if (cliIds.Count == 0)
                    return Enumerable.Empty<Tarefa>();

                if (usuarioId.HasValue)
                {
                    return await _tarefaRepository.BuscarTodosAsync(t =>
                        t.UsuId == usuarioId.Value && cliIds.Contains(t.CliId));
                }
                return await _tarefaRepository.BuscarTodosAsync(t => cliIds.Contains(t.CliId));
            }
            case "executor":
            {
                var upper = termo.ToUpperInvariant();
                var pessoas = await _pessoaRepository.BuscarTodosAsync(p =>
                    p.PesFantasia != null && p.PesFantasia.ToUpper().Contains(upper));
                var pesIds = pessoas.Select(p => p.PesId).ToHashSet();
                if (pesIds.Count == 0)
                    return Enumerable.Empty<Tarefa>();

                var usuarios = await _usuarioRepository.BuscarTodosAsync(u => pesIds.Contains(u.PesId));
                var usuIds = usuarios.Select(u => u.UsuId).ToHashSet();
                if (usuIds.Count == 0)
                    return Enumerable.Empty<Tarefa>();

                return await _tarefaRepository.BuscarTodosAsync(t => usuIds.Contains(t.UsuId));
            }
            case "status":
            {
                var upper = termo.ToUpperInvariant();
                var statusIds = new HashSet<int>();

                foreach (StatusTarefa st in Enum.GetValues(typeof(StatusTarefa)))
                {
                    var desc = st switch
                    {
                        StatusTarefa.EmAberto => "EM ABERTO",
                        StatusTarefa.Concluida => "CONCLUIDA",
                        StatusTarefa.Cancelada => "CANCELADA",
                        StatusTarefa.Reativada => "REATIVADA",
                        StatusTarefa.AguardandoCliente => "AGUARDANDO CLIENTE",
                        _ => st.ToString().ToUpperInvariant()
                    };
                    var descSemAcento = RemoverAcentos(desc);
                    var termoSemAcento = RemoverAcentos(upper);
                    if (desc.Contains(upper) || descSemAcento.Contains(termoSemAcento))
                        statusIds.Add((int)st);
                }

                var cadastros = await _cadastroStatusTarefaRepository.BuscarTodosAsync(s =>
                    s.Descricao != null && s.Descricao.ToUpper().Contains(upper));
                foreach (var c in cadastros)
                    statusIds.Add(c.Id);

                if (int.TryParse(termo, out var statusIdNumerico))
                    statusIds.Add(statusIdNumerico);

                if (statusIds.Count == 0)
                    return Enumerable.Empty<Tarefa>();

                if (usuarioId.HasValue)
                {
                    return await _tarefaRepository.BuscarTodosAsync(t =>
                        t.UsuId == usuarioId.Value && statusIds.Contains(t.TarStatus));
                }
                return await _tarefaRepository.BuscarTodosAsync(t => statusIds.Contains(t.TarStatus));
            }
            case "numero":
            {
                if (int.TryParse(termo, out var numero))
                {
                    if (usuarioId.HasValue)
                    {
                        return await _tarefaRepository.BuscarTodosAsync(t =>
                            t.UsuId == usuarioId.Value &&
                            (t.TarNumero == numero || t.TarId == numero ||
                             (t.TarNumero.HasValue && t.TarNumero.Value.ToString().Contains(termo))));
                    }
                    return await _tarefaRepository.BuscarTodosAsync(t =>
                        t.TarNumero == numero || t.TarId == numero ||
                        (t.TarNumero.HasValue && t.TarNumero.Value.ToString().Contains(termo)));
                }

                if (usuarioId.HasValue)
                {
                    var todas = await _tarefaRepository.BuscarTodosAsync(t => t.UsuId == usuarioId.Value);
                    return todas.Where(t =>
                        (t.TarNumero.HasValue && t.TarNumero.Value.ToString().Contains(termo)) ||
                        t.TarId.ToString().Contains(termo));
                }
                {
                    var todas = await _tarefaRepository.ListarTodosAsync();
                    return todas.Where(t =>
                        (t.TarNumero.HasValue && t.TarNumero.Value.ToString().Contains(termo)) ||
                        t.TarId.ToString().Contains(termo));
                }
            }
            case "data":
            {
                if (!DateTime.TryParse(termo, out var dataLocal))
                    return Enumerable.Empty<Tarefa>();

                var inicioUtc = DateTime.SpecifyKind(dataLocal.Date, DateTimeKind.Local).ToUniversalTime();
                var fimUtc = inicioUtc.AddDays(1).AddTicks(-1);

                if (usuarioId.HasValue)
                {
                    return await _tarefaRepository.BuscarTodosAsync(t =>
                        t.UsuId == usuarioId.Value &&
                        t.TarDtCadastro.HasValue &&
                        t.TarDtCadastro.Value >= inicioUtc &&
                        t.TarDtCadastro.Value <= fimUtc);
                }
                return await _tarefaRepository.BuscarTodosAsync(t =>
                    t.TarDtCadastro.HasValue &&
                    t.TarDtCadastro.Value >= inicioUtc &&
                    t.TarDtCadastro.Value <= fimUtc);
            }
            default:
                return Enumerable.Empty<Tarefa>();
        }
    }

    private static string RemoverAcentos(string texto)
    {
        var normalized = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    /// <summary>
    /// Normaliza o argumento de pesquisa estilo LIKE: "%" ou "%%" = todos; "%texto%" = "texto".
    /// </summary>
    private static string NormalizarTermoPesquisa(string? valor)
    {
        var termo = (valor ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(termo) || termo.All(c => c == '%'))
            return string.Empty;

        return termo.Trim('%').Trim();
    }

    public async Task<TarefaResponseDto> AtualizarTarefaAsync(int id, CadastroTarefaDto dto)
    {
        var tarefa = await _tarefaRepository.GetByIdAsync(id);
        if (tarefa == null)
            throw new InvalidOperationException("Tarefa não encontrada.");

        // Validar se cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        // Validar se usuário existe
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        // Atualizar Tarefa
        tarefa.CliId = dto.ClienteId;
        tarefa.UsuId = dto.UsuarioId;
        if (dto.DataCadastro.HasValue)
            tarefa.TarDtCadastro = DateTime.SpecifyKind(dto.DataCadastro.Value.Date, DateTimeKind.Utc);
        tarefa.TarDtConclusao = dto.DataConclusao?.ToUniversalTime();
        tarefa.TarStatus = dto.Status;
        tarefa.TarTitulo = dto.Titulo?.ToUpper();
        tarefa.TarProtocolo = dto.Protocolo?.ToUpper();
        tarefa.TarSolicitante = dto.Solicitante?.ToUpper();
        tarefa.TarCelularSolicitante = dto.CelularSolicitante;
        tarefa.TarTipoAtendimento = dto.TipoAtendimento.HasValue ? (TipoAtendimento?)dto.TipoAtendimento.Value : null;
        tarefa.TarPrioridade = dto.Prioridade;
        tarefa.TarTipoContato = dto.TipoContato.HasValue ? (TipoContato?)dto.TipoContato.Value : null;

        await _tarefaRepository.AtualizarAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        // Se houver descrição, criar anotação
        if (!string.IsNullOrWhiteSpace(dto.Descricao))
        {
            var anotacao = new AnotacaoTarefa
            {
                TarId = tarefa.TarId,
                UsuId = dto.UsuarioId,
                AntDescricao = dto.Descricao.ToUpper(),
                AntDtCadastro = DateTime.UtcNow
            };

            await _anotacaoRepository.InserirAsync(anotacao);
            await _anotacaoRepository.SalvarAlteracoesAsync();
        }

        // Salvar imagens se houver
        if (dto.Imagens != null && dto.Imagens.Count > 0)
        {
            foreach (var imagem in dto.Imagens)
            {
                if (imagem.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imagem.CopyToAsync(memoryStream);
                    var imagemBytes = memoryStream.ToArray();

                    var imagemTarefa = new ImagemTarefa
                    {
                        TarId = tarefa.TarId,
                        ImgArquivo = imagemBytes,
                        ImgDataArquivo = DateTime.UtcNow
                    };

                    await _imagemRepository.InserirAsync(imagemTarefa);
                }
            }
            await _imagemRepository.SalvarAlteracoesAsync();
        }

        return await MontarTarefaResponseDto(tarefa);
    }

    public async Task ExcluirTarefaAsync(int id)
    {
        var tarefa = await _tarefaRepository.GetByIdAsync(id);
        if (tarefa == null)
            throw new InvalidOperationException("Tarefa não encontrada.");

        await _tarefaRepository.ExcluirAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();
    }

    public async Task<TarefaResponseDto> AlterarStatusTarefaAsync(int id, int novoStatus)
    {
        var tarefa = await _tarefaRepository.GetByIdAsync(id);
        if (tarefa == null)
            throw new InvalidOperationException("Tarefa não encontrada.");

        tarefa.TarStatus = novoStatus;

        // Se concluída, definir data de conclusão
        if (novoStatus == StatusConcluidaId && tarefa.TarDtConclusao == null)
        {
            tarefa.TarDtConclusao = DateTime.UtcNow;
        }
        // Se não concluída, limpar data de conclusão
        else if (novoStatus != StatusConcluidaId)
        {
            tarefa.TarDtConclusao = null;
        }

        await _tarefaRepository.AtualizarAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        return await MontarTarefaResponseDto(tarefa);
    }

    private async Task<TarefaResponseDto> MontarTarefaResponseDto(Tarefa tarefa)
    {
        var cliente = await _clienteRepository.GetByIdAsync(tarefa.CliId);
        var usuario = await _usuarioRepository.GetByIdAsync(tarefa.UsuId);

        string clienteNome = "N/A";
        if (cliente != null)
        {
            var pessoaCliente = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            if (pessoaCliente != null)
            {
                // Mostrar código + fantasia
                clienteNome = $"{cliente.CliCodigo} - {pessoaCliente.PesFantasia ?? "N/A"}";
            }
        }

        string usuarioNome = "N/A";
        if (usuario != null)
        {
            var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);
            if (pessoaUsuario != null)
            {
                usuarioNome = pessoaUsuario.PesFantasia ?? "N/A";
            }
        }

        // Carregar anotações
        var anotacoes = await _anotacaoRepository.BuscarTodosAsync(a => a.TarId == tarefa.TarId);
        var anotacoesDto = new List<AnotacaoResponseDto>();

        foreach (var anotacao in anotacoes.OrderByDescending(a => a.AntDtCadastro))
        {
            var usuarioAnotacao = await _usuarioRepository.GetByIdAsync(anotacao.UsuId);
            string usuarioAnotacaoNome = "N/A";

            if (usuarioAnotacao != null)
            {
                var pessoaAnotacao = await _pessoaRepository.GetByIdAsync(usuarioAnotacao.PesId);
                if (pessoaAnotacao != null)
                {
                    usuarioAnotacaoNome = pessoaAnotacao.PesFantasia ?? "N/A";
                }
            }

            var dataCadastro = anotacao.AntDtCadastro?.ToLocalTime() ?? DateTime.Now;
            var descricaoFormatada = $"{usuarioAnotacaoNome} - {dataCadastro:dd/MM/yyyy - HH:mm} - {anotacao.AntDescricao}";

            anotacoesDto.Add(new AnotacaoResponseDto
            {
                AnotacaoId = anotacao.AntId,
                TarefaId = anotacao.TarId,
                UsuarioId = anotacao.UsuId,
                UsuarioNome = usuarioAnotacaoNome,
                Descricao = anotacao.AntDescricao ?? string.Empty,
                DataCadastro = anotacao.AntDtCadastro,
                DescricaoFormatada = descricaoFormatada
            });
        }

        // Carregar imagens
        var imagens = await _imagemRepository.BuscarTodosAsync(i => i.TarId == tarefa.TarId);
        var imagensDto = new List<ImagemResponseDto>();

        foreach (var imagem in imagens.OrderByDescending(i => i.ImgDataArquivo))
        {
            imagensDto.Add(new ImagemResponseDto
            {
                ImagemId = imagem.ImgId,
                TarefaId = imagem.TarId,
                UrlImagem = $"/api/imagem/{imagem.ImgId}",
                DataArquivo = imagem.ImgDataArquivo
            });
        }

        return new TarefaResponseDto
        {
            TarefaId = tarefa.TarId,
            ClienteId = tarefa.CliId,
            ClienteNome = clienteNome,
            UsuarioId = tarefa.UsuId,
            UsuarioNome = usuarioNome,
            DataCadastro = tarefa.TarDtCadastro,
            DataConclusao = tarefa.TarDtConclusao,
            Status = tarefa.TarStatus,
            StatusDescricao = await ObterDescricaoStatusAsync(tarefa.TarStatus),
            Titulo = tarefa.TarTitulo,
            Protocolo = tarefa.TarProtocolo,
            Solicitante = tarefa.TarSolicitante,
            CelularSolicitante = tarefa.TarCelularSolicitante,
            TipoAtendimento = tarefa.TarTipoAtendimento,
            TipoAtendimentoDescricao = await ObterDescricaoTipoAtendimentoAsync(tarefa.TarTipoAtendimento),
            Prioridade = tarefa.TarPrioridade,
            PrioridadeDescricao = ObterDescricaoPrioridade(tarefa.TarPrioridade),
            Numero = tarefa.TarNumero,
            TipoContato = tarefa.TarTipoContato,
            TipoContatoDescricao = await ObterDescricaoTipoContatoAsync(tarefa.TarTipoContato),
            Anotacoes = anotacoesDto,
            Imagens = imagensDto
        };
    }

    private async Task<string> ObterDescricaoStatusAsync(int statusId)
    {
        if (Enum.IsDefined(typeof(StatusTarefa), statusId))
        {
            return ((StatusTarefa)statusId) switch
            {
                StatusTarefa.EmAberto => "Em Aberto",
                StatusTarefa.Concluida => "Concluída",
                StatusTarefa.Cancelada => "Cancelada",
                StatusTarefa.Reativada => "Reativada",
                StatusTarefa.AguardandoCliente => "Aguardando Cliente",
                _ => statusId.ToString()
            };
        }

        var cadastro = await _cadastroStatusTarefaRepository.GetByIdAsync(statusId);
        return cadastro?.Descricao ?? statusId.ToString();
    }

    private async Task<string> ObterDescricaoTipoAtendimentoAsync(TipoAtendimento? tipo)
    {
        if (!tipo.HasValue) return string.Empty;
        if (Enum.IsDefined(typeof(TipoAtendimento), tipo.Value))
        {
            return tipo.Value switch
            {
                TipoAtendimento.Treinamento => "Treinamento",
                TipoAtendimento.Suporte => "Suporte",
                TipoAtendimento.Reuniao => "Reunião",
                TipoAtendimento.Cobranca => "Cobrança",
                _ => tipo.Value.ToString()
            };
        }

        var id = (int)tipo.Value;
        var cadastro = await _cadastroTipoAtendimentoRepository.GetByIdAsync(id);
        return cadastro?.Descricao ?? tipo.Value.ToString();
    }

    private string ObterDescricaoPrioridade(PrioridadeTarefa prioridade)
    {
        return prioridade switch
        {
            PrioridadeTarefa.Baixa => "Baixa",
            PrioridadeTarefa.Media => "Média",
            PrioridadeTarefa.Alta => "Alta",
            _ => prioridade.ToString()
        };
    }

    private async Task<string> ObterDescricaoTipoContatoAsync(TipoContato? tipo)
    {
        if (!tipo.HasValue) return string.Empty;
        if (Enum.IsDefined(typeof(TipoContato), tipo.Value))
        {
            return tipo.Value switch
            {
                TipoContato.Ligacao => "Ligação",
                TipoContato.WhatsApp => "WhatsApp",
                TipoContato.Email => "E-mail",
                _ => tipo.Value.ToString()
            };
        }

        var id = (int)tipo.Value;
        var cadastro = await _cadastroTipoContatoRepository.GetByIdAsync(id);
        return cadastro?.Descricao ?? tipo.Value.ToString();
    }

}
