using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class CentroCustoService : ICentroCustoService
{
    private readonly IRepository<CentroCusto> _centroCustoRepository;
    private readonly IRepository<Empresa> _empresaRepository;

    public CentroCustoService(
        IRepository<CentroCusto> centroCustoRepository,
        IRepository<Empresa> empresaRepository)
    {
        _centroCustoRepository = centroCustoRepository;
        _empresaRepository = empresaRepository;
    }

    private static CentroCustoResponseDto Mapear(CentroCusto centro, Empresa? empresa) => new()
    {
        CentroCustoId = centro.CcuId,
        EmpresaId = centro.EmpId,
        EmpresaFantasia = empresa?.EmpFantasia,
        EmpresaCnpj = empresa?.EmpCnpj
    };

    public async Task<CentroCustoResponseDto> CadastrarCentroCustoAsync(CadastroCentroCustoDto dto)
    {
        await ValidarEmpresaExistenteAsync(dto.EmpresaId);

        var centro = new CentroCusto { EmpId = dto.EmpresaId };
        await _centroCustoRepository.InserirAsync(centro);
        await _centroCustoRepository.SalvarAlteracoesAsync();

        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId);
        return Mapear(centro, empresa);
    }

    public async Task<CentroCustoResponseDto?> ObterCentroCustoPorIdAsync(int id)
    {
        var centro = await _centroCustoRepository.GetByIdAsync(id);
        if (centro == null) return null;

        var empresa = await _empresaRepository.GetByIdAsync(centro.EmpId);
        return Mapear(centro, empresa);
    }

    public async Task<IEnumerable<CentroCustoResponseDto>> ListarTodosCentrosCustoAsync()
    {
        var centros = await _centroCustoRepository.ListarTodosAsync();
        var empresas = (await _empresaRepository.ListarTodosAsync()).ToDictionary(e => e.EmpId);

        return centros
            .OrderBy(c => c.CcuId)
            .Select(c => Mapear(c, empresas.GetValueOrDefault(c.EmpId)));
    }

    public async Task<CentroCustoResponseDto> AtualizarCentroCustoAsync(int id, CadastroCentroCustoDto dto)
    {
        var centro = await _centroCustoRepository.GetByIdAsync(id);
        if (centro == null)
            throw new InvalidOperationException("Centro de custo não encontrado.");

        await ValidarEmpresaExistenteAsync(dto.EmpresaId);

        centro.EmpId = dto.EmpresaId;
        await _centroCustoRepository.AtualizarAsync(centro);
        await _centroCustoRepository.SalvarAlteracoesAsync();

        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId);
        return Mapear(centro, empresa);
    }

    public async Task ExcluirCentroCustoAsync(int id)
    {
        var centro = await _centroCustoRepository.GetByIdAsync(id);
        if (centro == null)
            throw new InvalidOperationException("Centro de custo não encontrado.");

        await _centroCustoRepository.ExcluirAsync(centro);
        await _centroCustoRepository.SalvarAlteracoesAsync();
    }

    private async Task ValidarEmpresaExistenteAsync(int empresaId)
    {
        var empresa = await _empresaRepository.GetByIdAsync(empresaId);
        if (empresa == null)
            throw new InvalidOperationException("Empresa informada não existe.");
    }
}
