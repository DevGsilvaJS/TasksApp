using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AnotacaoClienteService : IAnotacaoClienteService
{
    private readonly IRepository<AnotacaoCliente> _anotacaoClienteRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    public AnotacaoClienteService(
        IRepository<AnotacaoCliente> anotacaoClienteRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _anotacaoClienteRepository = anotacaoClienteRepository;
        _clienteRepository = clienteRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<AnotacaoClienteResponseDto> CadastrarAsync(CadastroAnotacaoClienteDto dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        var anotacao = new AnotacaoCliente
        {
            CliId = dto.ClienteId,
            AncDescricao = dto.Descricao,
            AncDtCadastro = DateTime.UtcNow
        };

        await _anotacaoClienteRepository.InserirAsync(anotacao);
        await _anotacaoClienteRepository.SalvarAlteracoesAsync();

        return await MontarResponseDtoAsync(anotacao);
    }

    public async Task<AnotacaoClienteResponseDto?> ObterPorIdAsync(int id)
    {
        var anotacao = await _anotacaoClienteRepository.GetByIdAsync(id);
        if (anotacao == null)
            return null;
        return await MontarResponseDtoAsync(anotacao);
    }

    public async Task<IEnumerable<AnotacaoClienteResponseDto>> ListarPorClienteAsync(int clienteId)
    {
        var anotacoes = await _anotacaoClienteRepository.BuscarTodosAsync(a => a.CliId == clienteId);
        var resultado = new List<AnotacaoClienteResponseDto>();
        foreach (var a in anotacoes.OrderByDescending(x => x.AncDtCadastro))
            resultado.Add(await MontarResponseDtoAsync(a));
        return resultado;
    }

    public async Task<IEnumerable<AnotacaoClienteResponseDto>> ListarTodasAsync()
    {
        var anotacoes = await _anotacaoClienteRepository.ListarTodosAsync();
        var resultado = new List<AnotacaoClienteResponseDto>();
        foreach (var a in anotacoes.OrderByDescending(x => x.AncDtCadastro))
            resultado.Add(await MontarResponseDtoAsync(a));
        return resultado;
    }

    public async Task<AnotacaoClienteResponseDto> AtualizarAsync(int id, CadastroAnotacaoClienteDto dto)
    {
        var anotacao = await _anotacaoClienteRepository.GetByIdAsync(id);
        if (anotacao == null)
            throw new InvalidOperationException("Anotação não encontrada.");

        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        anotacao.CliId = dto.ClienteId;
        anotacao.AncDescricao = dto.Descricao;

        await _anotacaoClienteRepository.AtualizarAsync(anotacao);
        await _anotacaoClienteRepository.SalvarAlteracoesAsync();

        return await MontarResponseDtoAsync(anotacao);
    }

    public async Task ExcluirAsync(int id)
    {
        var anotacao = await _anotacaoClienteRepository.GetByIdAsync(id);
        if (anotacao == null)
            throw new InvalidOperationException("Anotação não encontrada.");

        await _anotacaoClienteRepository.ExcluirAsync(anotacao);
        await _anotacaoClienteRepository.SalvarAlteracoesAsync();
    }

    private async Task<AnotacaoClienteResponseDto> MontarResponseDtoAsync(AnotacaoCliente anotacao)
    {
        string? fantasia = null;
        var cliente = await _clienteRepository.GetByIdAsync(anotacao.CliId);
        if (cliente != null)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(cliente.PesId);
            fantasia = pessoa?.PesFantasia;
        }

        return new AnotacaoClienteResponseDto
        {
            AnotacaoClienteId = anotacao.AncId,
            ClienteId = anotacao.CliId,
            Descricao = anotacao.AncDescricao,
            DataCadastro = anotacao.AncDtCadastro,
            ClienteFantasia = fantasia
        };
    }
}
