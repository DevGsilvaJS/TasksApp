using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System.Globalization;

namespace Application.Services;

public class ClienteService : IClienteService
{
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Email> _emailRepository;
    private readonly IRepository<ClienteContratoValor> _clienteContratoValorRepository;

    public ClienteService(
        IRepository<Pessoa> pessoaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Tarefa> tarefaRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Email> emailRepository,
        IRepository<ClienteContratoValor> clienteContratoValorRepository)
    {
        _pessoaRepository = pessoaRepository;
        _clienteRepository = clienteRepository;
        _tarefaRepository = tarefaRepository;
        _usuarioRepository = usuarioRepository;
        _emailRepository = emailRepository;
        _clienteContratoValorRepository = clienteContratoValorRepository;
    }

    public async Task<ClienteResponseDto> CadastrarClienteAsync(CadastroClienteDto dto)
    {
        // Verificar se o código já existe
        var codigoNormalizado = (dto.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigoNormalizado))
            throw new InvalidOperationException("Código do cliente é obrigatório.");

        var clienteExistente = await _clienteRepository.BuscarAsync(c => c.CliCodigo == codigoNormalizado);
        if (clienteExistente != null)
        {
            throw new InvalidOperationException("Código do cliente já está em uso. Por favor, escolha outro código.");
        }

        // Validar se usuário existe
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        // Criar Pessoa
        var pessoa = new Pessoa
        {
            PesFantasia = dto.Fantasia,
            PesDocFederal = dto.DocFederal,
            PesDocEstadual = dto.DocEstadual
        };

        await _pessoaRepository.InserirAsync(pessoa);
        await _pessoaRepository.SalvarAlteracoesAsync();

        // Criar Cliente
        var cliente = new Cliente
        {
            PesId = pessoa.PesId,
            UsuId = dto.UsuarioId,
            CliCodigo = codigoNormalizado,
            CliDataCadastro = DateTime.UtcNow,
            CliValorContrato = dto.ValorContrato,
            CliDataFinalContrato = dto.DataFinalContrato?.ToUniversalTime(),
            CliDiaPagamento = dto.DiaPagamento,
            CliDiaNfServico = dto.DiaNfServico,
            CliStatus = dto.Status
        };

        await _clienteRepository.InserirAsync(cliente);
        await _clienteRepository.SalvarAlteracoesAsync();

        await SalvarEmailsPessoaAsync(pessoa.PesId, dto.Emails);

        await SubstituirContratosAsync(cliente.CliId, dto);

        var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);
        var emails = await ObterEmailsPessoaAsync(pessoa.PesId);
        var (vigente, contratos) = await ObterContratosAsync(cliente.CliId);

        return new ClienteResponseDto
        {
            ClienteId = cliente.CliId,
            PessoaId = pessoa.PesId,
            Fantasia = pessoa.PesFantasia ?? string.Empty,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Codigo = cliente.CliCodigo,
            UsuarioId = cliente.UsuId,
            UsuarioNome = pessoaUsuario?.PesFantasia ?? string.Empty,
            DataCadastro = cliente.CliDataCadastro,
            ValorContrato = cliente.CliValorContrato,
            DataFinalContrato = cliente.CliDataFinalContrato,
            DiaPagamento = cliente.CliDiaPagamento,
            DiaNfServico = cliente.CliDiaNfServico,
            Status = cliente.CliStatus,
            StatusDescricao = ObterDescricaoStatus(cliente.CliStatus),
            Emails = emails,
            ValorContratoVigente = vigente?.CvcValorMensal,
            VigenciaInicio = vigente?.CvcDataInicio,
            VigenciaFim = vigente?.CvcDataFim,
            Contratos = contratos
        };
    }

    public async Task<ClienteResponseDto?> ObterClientePorIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            return null;

        var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
        if (pessoa == null)
            return null;

        var usuario = await _usuarioRepository.GetByIdAsync(cliente.UsuId);
        var pessoaUsuario = usuario != null ? await _pessoaRepository.GetByIdAsync(usuario.PesId) : null;

        var emails = await ObterEmailsPessoaAsync(pessoa.PesId);
        var (vigente, contratos) = await ObterContratosAsync(cliente.CliId);
        return new ClienteResponseDto
        {
            ClienteId = cliente.CliId,
            PessoaId = pessoa.PesId,
            Fantasia = pessoa.PesFantasia ?? string.Empty,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Codigo = cliente.CliCodigo,
            UsuarioId = cliente.UsuId,
            UsuarioNome = pessoaUsuario?.PesFantasia ?? string.Empty,
            DataCadastro = cliente.CliDataCadastro,
            ValorContrato = cliente.CliValorContrato,
            DataFinalContrato = cliente.CliDataFinalContrato,
            DiaPagamento = cliente.CliDiaPagamento,
            DiaNfServico = cliente.CliDiaNfServico,
            Status = cliente.CliStatus,
            StatusDescricao = ObterDescricaoStatus(cliente.CliStatus),
            Emails = emails,
            ValorContratoVigente = vigente?.CvcValorMensal,
            VigenciaInicio = vigente?.CvcDataInicio,
            VigenciaFim = vigente?.CvcDataFim,
            Contratos = contratos
        };
    }

    public async Task<IEnumerable<ClienteResponseDto>> ListarTodosClientesAsync()
    {
        var clientes = await _clienteRepository.ListarTodosAsync();
        var resultado = new List<ClienteResponseDto>();

        foreach (var cliente in clientes)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            if (pessoa != null)
            {
                var usuario = await _usuarioRepository.GetByIdAsync(cliente.UsuId);
                var pessoaUsuario = usuario != null ? await _pessoaRepository.GetByIdAsync(usuario.PesId) : null;

                var emails = await ObterEmailsPessoaAsync(pessoa.PesId);
                var (vigente, contratos) = await ObterContratosAsync(cliente.CliId);
                resultado.Add(new ClienteResponseDto
                {
                    ClienteId = cliente.CliId,
                    PessoaId = pessoa.PesId,
                    Fantasia = pessoa.PesFantasia ?? string.Empty,
                    DocFederal = pessoa.PesDocFederal,
                    DocEstadual = pessoa.PesDocEstadual,
                    Codigo = cliente.CliCodigo,
                    UsuarioId = cliente.UsuId,
                    UsuarioNome = pessoaUsuario?.PesFantasia ?? string.Empty,
                    DataCadastro = cliente.CliDataCadastro,
                    ValorContrato = cliente.CliValorContrato,
                    DataFinalContrato = cliente.CliDataFinalContrato,
                    DiaPagamento = cliente.CliDiaPagamento,
                    DiaNfServico = cliente.CliDiaNfServico,
                    Status = cliente.CliStatus,
                    StatusDescricao = ObterDescricaoStatus(cliente.CliStatus),
                    Emails = emails,
                    ValorContratoVigente = vigente?.CvcValorMensal,
                    VigenciaInicio = vigente?.CvcDataInicio,
                    VigenciaFim = vigente?.CvcDataFim,
                    Contratos = contratos
                });
            }
        }

        return resultado.OrderBy(
            c => c.Codigo,
            StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: true));
    }

    public async Task<ClienteResponseDto> AtualizarClienteAsync(int id, CadastroClienteDto dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        // Verificar se o código já existe em outro cliente
        var codigoNormalizadoAtualizacao = (dto.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigoNormalizadoAtualizacao))
            throw new InvalidOperationException("Código do cliente é obrigatório.");

        var clienteComCodigo = await _clienteRepository.BuscarAsync(c => c.CliCodigo == codigoNormalizadoAtualizacao && c.CliId != id);
        if (clienteComCodigo != null)
        {
            throw new InvalidOperationException("Código do cliente já está em uso por outro cliente.");
        }

        // Validar se usuário existe
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
        if (pessoa == null)
            throw new InvalidOperationException("Pessoa associada ao cliente não encontrada.");

        // Atualizar Pessoa
        pessoa.PesFantasia = dto.Fantasia;
        pessoa.PesDocFederal = dto.DocFederal;
        pessoa.PesDocEstadual = dto.DocEstadual;

        await _pessoaRepository.AtualizarAsync(pessoa);

        // Atualizar Cliente
        cliente.CliCodigo = codigoNormalizadoAtualizacao;
        cliente.UsuId = dto.UsuarioId;
        cliente.CliValorContrato = dto.ValorContrato;
        cliente.CliDataFinalContrato = dto.DataFinalContrato?.ToUniversalTime();
        cliente.CliDiaPagamento = dto.DiaPagamento;
        cliente.CliDiaNfServico = dto.DiaNfServico;
        cliente.CliStatus = dto.Status;

        await _clienteRepository.AtualizarAsync(cliente);
        await SalvarEmailsPessoaAsync(pessoa.PesId, dto.Emails);
        await _clienteRepository.SalvarAlteracoesAsync();

        await SubstituirContratosAsync(cliente.CliId, dto);

        var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);
        var emails = await ObterEmailsPessoaAsync(pessoa.PesId);
        var (vigente, contratos) = await ObterContratosAsync(cliente.CliId);

        return new ClienteResponseDto
        {
            ClienteId = cliente.CliId,
            PessoaId = pessoa.PesId,
            Fantasia = pessoa.PesFantasia ?? string.Empty,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Codigo = cliente.CliCodigo,
            UsuarioId = cliente.UsuId,
            UsuarioNome = pessoaUsuario?.PesFantasia ?? string.Empty,
            DataCadastro = cliente.CliDataCadastro,
            ValorContrato = cliente.CliValorContrato,
            DataFinalContrato = cliente.CliDataFinalContrato,
            DiaPagamento = cliente.CliDiaPagamento,
            DiaNfServico = cliente.CliDiaNfServico,
            Status = cliente.CliStatus,
            StatusDescricao = ObterDescricaoStatus(cliente.CliStatus),
            Emails = emails,
            ValorContratoVigente = vigente?.CvcValorMensal,
            VigenciaInicio = vigente?.CvcDataInicio,
            VigenciaFim = vigente?.CvcDataFim,
            Contratos = contratos
        };
    }

    private async Task SubstituirContratosAsync(int clienteId, CadastroClienteDto dto)
    {
        var contratosNovos = dto.Contratos;

        if (contratosNovos == null || contratosNovos.Count == 0)
        {
            // Compatibilidade: se o cliente ainda usa os campos antigos, cria uma vigência única.
            if (dto.ValorContrato.HasValue && dto.ValorContrato.Value > 0)
            {
                var existentesLegado = await _clienteContratoValorRepository.BuscarTodosAsync(c => c.CliId == clienteId);
                foreach (var c in existentesLegado)
                    await _clienteContratoValorRepository.ExcluirAsync(c);

                await _clienteContratoValorRepository.InserirAsync(new ClienteContratoValor
                {
                    CliId = clienteId,
                    CvcValorMensal = dto.ValorContrato.Value,
                    CvcDataInicio = DateTime.UtcNow.Date,
                    CvcDataFim = dto.DataFinalContrato?.ToUniversalTime().Date
                });
            }

            await _clienteContratoValorRepository.SalvarAlteracoesAsync();
            return;
        }

        ValidarSobreposicaoVigencias(contratosNovos);

        var existentes = await _clienteContratoValorRepository.BuscarTodosAsync(c => c.CliId == clienteId);
        foreach (var c in existentes)
            await _clienteContratoValorRepository.ExcluirAsync(c);

        foreach (var contrato in contratosNovos)
        {
            if (contrato.ValorMensal <= 0) continue;
            await _clienteContratoValorRepository.InserirAsync(new ClienteContratoValor
            {
                CliId = clienteId,
                CvcValorMensal = contrato.ValorMensal,
                CvcDataInicio = contrato.DataInicio.ToUniversalTime().Date,
                CvcDataFim = contrato.DataFim?.ToUniversalTime().Date
            });
        }

        await _clienteContratoValorRepository.SalvarAlteracoesAsync();
    }

    private static void ValidarSobreposicaoVigencias(List<ClienteContratoValorDto> contratos)
    {
        var ordenados = contratos
            .Select((c, idx) => new
            {
                Index = idx,
                Inicio = c.DataInicio.Date,
                Fim = c.DataFim?.Date,
                c.ValorMensal
            })
            .Where(x => x.ValorMensal > 0)
            .OrderBy(x => x.Inicio)
            .ToList();

        for (var i = 0; i < ordenados.Count; i++)
        {
            var atual = ordenados[i];
            if (atual.Fim.HasValue && atual.Fim.Value < atual.Inicio)
                throw new InvalidOperationException($"Contrato inválido: a data fim é menor que a data início (linha {atual.Index + 1}).");
        }

        for (var i = 0; i < ordenados.Count - 1; i++)
        {
            var a = ordenados[i];
            var b = ordenados[i + 1];

            // Sobreposição inclusiva: se A não tem fim, ele ocupa tudo.
            // Se tem fim, não pode existir B iniciando em data <= fim de A.
            if (!a.Fim.HasValue || b.Inicio <= a.Fim.Value)
            {
                throw new InvalidOperationException(
                    $"Não é permitido ter 2 contratos com vigência no mesmo período. Conflito entre as linhas {a.Index + 1} e {b.Index + 1}.");
            }
        }
    }

    private async Task<(ClienteContratoValor? Vigente, List<ClienteContratoValorResponseDto> Contratos)> ObterContratosAsync(int clienteId)
    {
        List<ClienteContratoValor> contratos;
        try
        {
            contratos = (await _clienteContratoValorRepository.BuscarTodosAsync(c => c.CliId == clienteId))
                .OrderByDescending(c => c.CvcDataInicio)
                .ToList();
        }
        catch
        {
            // Banco ainda sem a tabela de contratos. Mantém compatibilidade com o modelo antigo.
            return (null, new List<ClienteContratoValorResponseDto>());
        }

        var hoje = DateTime.UtcNow.Date;
        var vigente = contratos.FirstOrDefault(c => c.CvcDataInicio.Date <= hoje && (!c.CvcDataFim.HasValue || c.CvcDataFim.Value.Date >= hoje));

        var response = contratos.Select(c => new ClienteContratoValorResponseDto
        {
            ContratoId = c.CvcId,
            ValorMensal = c.CvcValorMensal,
            DataInicio = c.CvcDataInicio,
            DataFim = c.CvcDataFim
        }).ToList();

        return (vigente, response);
    }

    private async Task<List<string>> ObterEmailsPessoaAsync(int pesId)
    {
        var emails = await _emailRepository.BuscarTodosAsync(e => e.PesId == pesId);
        return emails
            .Select(e => e.EmlDescricao ?? string.Empty)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private async Task SalvarEmailsPessoaAsync(int pesId, List<string>? novosEmails)
    {
        var existentes = await _emailRepository.BuscarTodosAsync(e => e.PesId == pesId);
        foreach (var email in existentes)
            await _emailRepository.ExcluirAsync(email);

        if (novosEmails != null)
        {
            foreach (var descricao in novosEmails.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                await _emailRepository.InserirAsync(new Email
                {
                    PesId = pesId,
                    EmlDescricao = descricao.Trim()
                });
            }
        }

        await _emailRepository.SalvarAlteracoesAsync();
    }

    public async Task ExcluirClienteAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        // Verificar se existem tarefas associadas a este cliente
        var tarefas = await _tarefaRepository.BuscarTodosAsync(t => t.CliId == cliente.CliId);
        if (tarefas != null && tarefas.Any())
        {
            throw new InvalidOperationException($"Não é possível excluir o cliente. Existem {tarefas.Count()} tarefa(s) associada(s) a este cliente.");
        }

        var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
        
        await _clienteRepository.ExcluirAsync(cliente);
        await _clienteRepository.SalvarAlteracoesAsync();

        if (pessoa != null)
        {
            // Verificar se a pessoa não é usada por outro relacionamento
            var temUsuario = await _pessoaRepository.BuscarAsync(p => p.PesId == pessoa.PesId && p.Usuario != null);
            if (temUsuario == null)
            {
                await _pessoaRepository.ExcluirAsync(pessoa);
                await _pessoaRepository.SalvarAlteracoesAsync();
            }
        }
    }

    private string ObterDescricaoStatus(StatusCliente status)
    {
        return status switch
        {
            StatusCliente.Ativo => "Ativo",
            StatusCliente.Inativo => "Inativo",
            StatusCliente.Suspenso => "Suspenso",
            _ => status.ToString()
        };
    }
}
