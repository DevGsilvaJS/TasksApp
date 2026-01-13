using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ClienteService : IClienteService
{
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Tarefa> _tarefaRepository;

    public ClienteService(
        IRepository<Pessoa> pessoaRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Tarefa> tarefaRepository)
    {
        _pessoaRepository = pessoaRepository;
        _clienteRepository = clienteRepository;
        _tarefaRepository = tarefaRepository;
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
                CliCodigo = dto.Codigo,
                CliDataCadastro = DateTime.UtcNow
            };

            await _clienteRepository.InserirAsync(cliente);
            await _clienteRepository.SalvarAlteracoesAsync();

            return new ClienteResponseDto
            {
                ClienteId = cliente.CliId,
                PessoaId = pessoa.PesId,
                Fantasia = pessoa.PesFantasia ?? string.Empty,
                DocFederal = pessoa.PesDocFederal,
                DocEstadual = pessoa.PesDocEstadual,
                Codigo = cliente.CliCodigo,
                DataCadastro = cliente.CliDataCadastro
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

        return new ClienteResponseDto
        {
            ClienteId = cliente.CliId,
            PessoaId = pessoa.PesId,
            Fantasia = pessoa.PesFantasia ?? string.Empty,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Codigo = cliente.CliCodigo,
            DataCadastro = cliente.CliDataCadastro
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
                resultado.Add(new ClienteResponseDto
                {
                    ClienteId = cliente.CliId,
                    PessoaId = pessoa.PesId,
                    Fantasia = pessoa.PesFantasia ?? string.Empty,
                    DocFederal = pessoa.PesDocFederal,
                    DocEstadual = pessoa.PesDocEstadual,
                    Codigo = cliente.CliCodigo,
                    DataCadastro = cliente.CliDataCadastro
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

        await _clienteRepository.AtualizarAsync(cliente);
        await _clienteRepository.SalvarAlteracoesAsync();

        return new ClienteResponseDto
        {
            ClienteId = cliente.CliId,
            PessoaId = pessoa.PesId,
            Fantasia = pessoa.PesFantasia ?? string.Empty,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Codigo = cliente.CliCodigo,
            DataCadastro = cliente.CliDataCadastro
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
}
