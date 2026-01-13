using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TarefaService : ITarefaService
{
    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<AnotacaoTarefa> _anotacaoRepository;
    private readonly IRepository<ImagemTarefa> _imagemRepository;

    public TarefaService(
        IRepository<Tarefa> tarefaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Pessoa> pessoaRepository,
        IRepository<AnotacaoTarefa> anotacaoRepository,
        IRepository<ImagemTarefa> imagemRepository)
    {
        _tarefaRepository = tarefaRepository;
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _pessoaRepository = pessoaRepository;
        _anotacaoRepository = anotacaoRepository;
        _imagemRepository = imagemRepository;
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

        // Criar Tarefa
        var tarefa = new Tarefa
        {
            CliId = dto.ClienteId,
            UsuId = dto.UsuarioId,
            TarDtCadastro = DateTime.UtcNow,
            TarDtConclusao = dto.DataConclusao?.ToUniversalTime(),
            TarStatus = dto.Status,
            TarTitulo = dto.Titulo,
            TarProtocolo = dto.Protocolo,
            TarSolicitante = dto.Solicitante
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
                AntDescricao = dto.Descricao,
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
        var tarefas = await _tarefaRepository.ListarTodosAsync();
        var resultado = new List<TarefaResponseDto>();

        foreach (var tarefa in tarefas)
        {
            resultado.Add(await MontarTarefaResponseDto(tarefa));
        }

        return resultado;
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
        tarefa.TarDtConclusao = dto.DataConclusao?.ToUniversalTime();
        tarefa.TarStatus = dto.Status;
        tarefa.TarTitulo = dto.Titulo;
        tarefa.TarProtocolo = dto.Protocolo;
        tarefa.TarSolicitante = dto.Solicitante;

        await _tarefaRepository.AtualizarAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        // Se houver descrição, criar anotação
        if (!string.IsNullOrWhiteSpace(dto.Descricao))
        {
            var anotacao = new AnotacaoTarefa
            {
                TarId = tarefa.TarId,
                UsuId = dto.UsuarioId,
                AntDescricao = dto.Descricao,
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

    public async Task<TarefaResponseDto> AlterarStatusTarefaAsync(int id, StatusTarefa novoStatus)
    {
        var tarefa = await _tarefaRepository.GetByIdAsync(id);
        if (tarefa == null)
            throw new InvalidOperationException("Tarefa não encontrada.");

        tarefa.TarStatus = novoStatus;

        // Se concluída, definir data de conclusão
        if (novoStatus == StatusTarefa.Concluida && tarefa.TarDtConclusao == null)
        {
            tarefa.TarDtConclusao = DateTime.UtcNow;
        }
        // Se não concluída, limpar data de conclusão
        else if (novoStatus != StatusTarefa.Concluida)
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
            var descricaoFormatada = $"{dataCadastro:dd/MM/yyyy - HH:mm} - {anotacao.AntDescricao}";

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
            StatusDescricao = ObterDescricaoStatus(tarefa.TarStatus),
            Titulo = tarefa.TarTitulo,
            Protocolo = tarefa.TarProtocolo,
            Solicitante = tarefa.TarSolicitante,
            Anotacoes = anotacoesDto,
            Imagens = imagensDto
        };
    }

    private string ObterDescricaoStatus(StatusTarefa status)
    {
        return status switch
        {
            StatusTarefa.EmAberto => "Em Aberto",
            StatusTarefa.Concluida => "Concluída",
            StatusTarefa.Cancelada => "Cancelada",
            _ => status.ToString()
        };
    }

}
