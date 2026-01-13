using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AnotacaoService : IAnotacaoService
{
    private readonly IRepository<AnotacaoTarefa> _anotacaoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    public AnotacaoService(
        IRepository<AnotacaoTarefa> anotacaoRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _anotacaoRepository = anotacaoRepository;
        _usuarioRepository = usuarioRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<AnotacaoResponseDto> CadastrarAnotacaoAsync(CadastroAnotacaoDto dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var anotacao = new AnotacaoTarefa
        {
            TarId = dto.TarefaId,
            UsuId = dto.UsuarioId,
            AntDescricao = dto.Descricao,
            AntDtCadastro = DateTime.UtcNow
        };

        await _anotacaoRepository.InserirAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();

        return await MontarAnotacaoResponseDto(anotacao);
    }

    public async Task<IEnumerable<AnotacaoResponseDto>> ObterAnotacoesPorTarefaAsync(int tarefaId)
    {
        var anotacoes = await _anotacaoRepository.BuscarTodosAsync(a => a.TarId == tarefaId);
        var resultado = new List<AnotacaoResponseDto>();

        foreach (var anotacao in anotacoes.OrderByDescending(a => a.AntDtCadastro))
        {
            resultado.Add(await MontarAnotacaoResponseDto(anotacao));
        }

        return resultado;
    }

    public async Task ExcluirAnotacaoAsync(int id)
    {
        var anotacao = await _anotacaoRepository.GetByIdAsync(id);
        if (anotacao == null)
            throw new InvalidOperationException("Anotação não encontrada.");

        await _anotacaoRepository.ExcluirAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();
    }

    private async Task<AnotacaoResponseDto> MontarAnotacaoResponseDto(AnotacaoTarefa anotacao)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(anotacao.UsuId);
        string usuarioNome = "N/A";

        if (usuario != null)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
            if (pessoa != null)
            {
                usuarioNome = pessoa.PesFantasia ?? "N/A";
            }
        }

        var dataCadastro = anotacao.AntDtCadastro?.ToLocalTime() ?? DateTime.Now;
        var descricaoFormatada = $"{dataCadastro:dd/MM/yyyy - HH:mm} - {anotacao.AntDescricao}";

        return new AnotacaoResponseDto
        {
            AnotacaoId = anotacao.AntId,
            TarefaId = anotacao.TarId,
            UsuarioId = anotacao.UsuId,
            UsuarioNome = usuarioNome,
            Descricao = anotacao.AntDescricao ?? string.Empty,
            DataCadastro = anotacao.AntDtCadastro,
            DescricaoFormatada = descricaoFormatada
        };
    }
}
