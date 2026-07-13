using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AnotacaoGeralService : IAnotacaoGeralService
{
    public const string TipoAnotacao = "ANOTACAO";
    public const string TipoRegraEmpresa = "REGRA_EMPRESA";

    private readonly IRepository<Anotacao> _anotacaoRepository;

    public AnotacaoGeralService(IRepository<Anotacao> anotacaoRepository)
    {
        _anotacaoRepository = anotacaoRepository;
    }

    public async Task<AnotacaoGeralResponseDto> CadastrarAnotacaoAsync(CadastroAnotacaoGeralDto dto)
    {
        var tipo = NormalizarTipo(dto.Tipo);
        var anotacao = new Anotacao
        {
            AnoDescricao = dto.Descricao,
            AnoObservacoes = tipo == TipoRegraEmpresa ? dto.Observacoes : null,
            AnoLink = tipo == TipoAnotacao ? dto.Link : null,
            AnoTipo = tipo,
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

        var tipo = NormalizarTipo(dto.Tipo);
        anotacao.AnoDescricao = dto.Descricao;
        anotacao.AnoTipo = tipo;
        anotacao.AnoObservacoes = tipo == TipoRegraEmpresa ? dto.Observacoes : null;
        anotacao.AnoLink = tipo == TipoAnotacao ? dto.Link : null;

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

    private static string NormalizarTipo(string? tipo)
    {
        if (string.Equals(tipo, TipoRegraEmpresa, StringComparison.OrdinalIgnoreCase))
            return TipoRegraEmpresa;

        return TipoAnotacao;
    }

    private static AnotacaoGeralResponseDto MontarAnotacaoResponseDto(Anotacao anotacao)
    {
        var tipo = string.IsNullOrWhiteSpace(anotacao.AnoTipo) ? TipoAnotacao : anotacao.AnoTipo.ToUpperInvariant();
        if (tipo != TipoRegraEmpresa)
            tipo = TipoAnotacao;

        return new AnotacaoGeralResponseDto
        {
            AnotacaoId = anotacao.AnoId,
            Descricao = anotacao.AnoDescricao ?? string.Empty,
            Observacoes = anotacao.AnoObservacoes,
            Link = anotacao.AnoLink,
            Tipo = tipo,
            DataCadastro = anotacao.AnoDtCadastro
        };
    }
}
