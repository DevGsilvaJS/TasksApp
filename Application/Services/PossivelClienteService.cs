using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class PossivelClienteService : IPossivelClienteService
{
    private readonly IRepository<PossivelCliente> _repository;
    private readonly IRepository<PossivelClienteAnotacao> _anotacaoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Pessoa> _pessoaRepository;

    public PossivelClienteService(
        IRepository<PossivelCliente> repository,
        IRepository<PossivelClienteAnotacao> anotacaoRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Pessoa> pessoaRepository)
    {
        _repository = repository;
        _anotacaoRepository = anotacaoRepository;
        _usuarioRepository = usuarioRepository;
        _pessoaRepository = pessoaRepository;
    }

    public async Task<IEnumerable<PossivelClienteResponseDto>> ListarTodosAsync()
    {
        var lista = await _repository.BuscarTodosAsync(p => p.PocStatus == "1 - OK");
        return lista.Select(MapearParaDto);
    }

    public async Task<PossivelClienteResponseDto?> ObterPorIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapearParaDto(entity);
    }

    private static PossivelClienteResponseDto MapearParaDto(PossivelCliente p)
    {
        return new PossivelClienteResponseDto
        {
            PocId = p.PocId,
            PocCodigo = p.PocCodigo,
            PocLoja = p.PocLoja,
            PocStatus = p.PocStatus,
            PocFantasia = p.PocFantasia,
            PocDdd = p.PocDdd,
            PocCnpj = p.PocCnpj,
            PocRazaoSocial = p.PocRazaoSocial,
            PocEmailComercial = p.PocEmailComercial,
            PocCelDdd = p.PocCelDdd,
            PocCelular = p.PocCelular,
            PocDataImportacao = p.PocDataImportacao,
            PocStatusAtendimento = p.PocStatusAtendimento,
            PocMotivoPerda = p.PocMotivoPerda,
            PocDataStatusAtendimento = p.PocDataStatusAtendimento
        };
    }

    public async Task<PossivelClienteResponseDto?> AtualizarStatusAtendimentoAsync(int id, AtualizarStatusAtendimentoDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.StatusAtendimento < 1 || dto.StatusAtendimento > 9)
            throw new ArgumentOutOfRangeException(nameof(dto.StatusAtendimento), "Status deve ser entre 1 e 9.");
        entity.PocStatusAtendimento = dto.StatusAtendimento;
        entity.PocMotivoPerda = dto.StatusAtendimento == 8 ? dto.MotivoPerda : null;
        entity.PocDataStatusAtendimento = DateTime.UtcNow;
        await _repository.AtualizarAsync(entity);
        await _repository.SalvarAlteracoesAsync();
        return MapearParaDto(entity);
    }

    public async Task<IEnumerable<PossivelClienteAnotacaoResponseDto>> ListarAnotacoesAsync(int pocId)
    {
        var anotacoes = await _anotacaoRepository.BuscarTodosAsync(a => a.PocId == pocId);
        var resultado = new List<PossivelClienteAnotacaoResponseDto>();
        foreach (var a in anotacoes.OrderByDescending(x => x.PcaDtCadastro))
            resultado.Add(await MapearAnotacaoParaDtoAsync(a));
        return resultado;
    }

    public async Task<PossivelClienteAnotacaoResponseDto> AdicionarAnotacaoAsync(int pocId, CadastroPossivelClienteAnotacaoDto dto)
    {
        var possivelCliente = await _repository.GetByIdAsync(pocId);
        if (possivelCliente == null)
            throw new InvalidOperationException("Possível cliente não encontrado.");
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var anotacao = new PossivelClienteAnotacao
        {
            PocId = pocId,
            UsuId = dto.UsuarioId,
            PcaDescricao = dto.Descricao,
            PcaDtCadastro = DateTime.UtcNow
        };
        await _anotacaoRepository.InserirAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();
        return await MapearAnotacaoParaDtoAsync(anotacao);
    }

    public async Task<PossivelClienteAnotacaoResponseDto?> AtualizarAnotacaoAsync(int pocId, int anotacaoId, AtualizarPossivelClienteAnotacaoDto dto)
    {
        var anotacao = await _anotacaoRepository.GetByIdAsync(anotacaoId);
        if (anotacao == null || anotacao.PocId != pocId)
            return null;
        anotacao.PcaDescricao = dto.Descricao;
        await _anotacaoRepository.AtualizarAsync(anotacao);
        await _anotacaoRepository.SalvarAlteracoesAsync();
        return await MapearAnotacaoParaDtoAsync(anotacao);
    }

    private async Task<PossivelClienteAnotacaoResponseDto> MapearAnotacaoParaDtoAsync(PossivelClienteAnotacao a)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(a.UsuId);
        var pessoa = usuario != null ? await _pessoaRepository.GetByIdAsync(usuario.PesId) : null;
        return new PossivelClienteAnotacaoResponseDto
        {
            PcaId = a.PcaId,
            PocId = a.PocId,
            UsuId = a.UsuId,
            UsuarioNome = pessoa?.PesFantasia,
            Descricao = a.PcaDescricao,
            DataCadastro = a.PcaDtCadastro
        };
    }
}
