using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ClienteService : IClienteService
{
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Tarefa> _tarefaRepository;
    private readonly IRepository<Usuario> _usuarioRepository;

    public ClienteService(
        IRepository<Pessoa> pessoaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Tarefa> tarefaRepository,
        IRepository<Usuario> usuarioRepository)
    {
        _pessoaRepository = pessoaRepository;
        _clienteRepository = clienteRepository;
        _tarefaRepository = tarefaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ClienteResponseDto> CadastrarClienteAsync(CadastroClienteDto dto)
    {
        try
        {
            // Verificar se o código já existe
            var clienteExistente = await _clienteRepository.BuscarAsync(c => c.CliCodigo == dto.Codigo);
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
                CliCodigo = dto.Codigo,
                CliDataCadastro = DateTime.UtcNow,
                CliValorContrato = dto.ValorContrato,
                CliDataFinalContrato = dto.DataFinalContrato?.ToUniversalTime(),
                CliDiaPagamento = dto.DiaPagamento,
                CliStatus = dto.Status
            };

            await _clienteRepository.InserirAsync(cliente);
            await _clienteRepository.SalvarAlteracoesAsync();

            var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);

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
                Status = cliente.CliStatus,
                StatusDescricao = ObterDescricaoStatus(cliente.CliStatus)
            };
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }
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
            Status = cliente.CliStatus,
            StatusDescricao = ObterDescricaoStatus(cliente.CliStatus)
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
                    Status = cliente.CliStatus,
                    StatusDescricao = ObterDescricaoStatus(cliente.CliStatus)
                });
            }
        }

        return resultado;
    }

    public async Task<ClienteResponseDto> AtualizarClienteAsync(int id, CadastroClienteDto dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        // Verificar se o código já existe em outro cliente
        var clienteComCodigo = await _clienteRepository.BuscarAsync(c => c.CliCodigo == dto.Codigo && c.CliId != id);
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
        cliente.CliCodigo = dto.Codigo;
        cliente.UsuId = dto.UsuarioId;
        cliente.CliValorContrato = dto.ValorContrato;
        cliente.CliDataFinalContrato = dto.DataFinalContrato?.ToUniversalTime();
        cliente.CliDiaPagamento = dto.DiaPagamento;
        cliente.CliStatus = dto.Status;

        await _clienteRepository.AtualizarAsync(cliente);
        await _clienteRepository.SalvarAlteracoesAsync();

        var pessoaUsuario = await _pessoaRepository.GetByIdAsync(usuario.PesId);

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
            Status = cliente.CliStatus,
            StatusDescricao = ObterDescricaoStatus(cliente.CliStatus)
        };
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
