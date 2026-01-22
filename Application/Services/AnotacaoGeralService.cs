using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AnotacaoGeralService : IAnotacaoGeralService
{
    private readonly IRepository<Anotacao> _anotacaoRepository;

    public AnotacaoGeralService(IRepository<Anotacao> anotacaoRepository)
    {
        _anotacaoRepository = anotacaoRepository;
    }

    public async Task<AnotacaoGeralResponseDto> CadastrarAnotacaoAsync(CadastroAnotacaoGeralDto dto)
    {
        var anotacao = new Anotacao
        {
            AnoDescricao = dto.Descricao,
            AnoLink = dto.Link,
            AnoDtCadastro = DateTime.UtcNow
        };

        await _anotacaoRepository.InserirAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();

        return MontarAnotacaoResponseDto(anotacao);
    }

    public async Task<IEnumerable<AnotacaoGeralResponseDto>> ListarTodasAnotacoesAsync()
    {
        var anotacoes = await _anotacaoRepository.ListarTodosAsync();
        return anotacoes.OrderByDescending(a => a.AnoDtCadastro)
            .Select(a => MontarAnotacaoResponseDto(a))
            .ToList();
    }

    public async Task<AnotacaoGeralResponseDto?> ObterAnotacaoPorIdAsync(int id)
    {
        var anotacao = await _anotacaoRepository.GetByIdAsync(id);
        if (anotacao == null)
            return null;

        return MontarAnotacaoResponseDto(anotacao);
    }

    public async Task<AnotacaoGeralResponseDto> AtualizarAnotacaoAsync(int id, CadastroAnotacaoGeralDto dto)
    {
        var anotacao = await _anotacaoRepository.GetByIdAsync(id);
        if (anotacao == null)
            throw new InvalidOperationException("Anotação não encontrada.");

        anotacao.AnoDescricao = dto.Descricao;
        anotacao.AnoLink = dto.Link;

        await _anotacaoRepository.AtualizarAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();

        return MontarAnotacaoResponseDto(anotacao);
    }

    public async Task ExcluirAnotacaoAsync(int id)
    {
        var anotacao = await _anotacaoRepository.GetByIdAsync(id);
        if (anotacao == null)
            throw new InvalidOperationException("Anotação não encontrada.");

        await _anotacaoRepository.ExcluirAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();
    }

    private AnotacaoGeralResponseDto MontarAnotacaoResponseDto(Anotacao anotacao)
    {
        return new AnotacaoGeralResponseDto
        {
            AnotacaoId = anotacao.AnoId,
            Descricao = anotacao.AnoDescricao ?? string.Empty,
            Link = anotacao.AnoLink,
            DataCadastro = anotacao.AnoDtCadastro
        };
    }
}
