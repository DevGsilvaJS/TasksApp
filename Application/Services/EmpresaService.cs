using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<CentroCusto> _centroCustoRepository;

    public EmpresaService(
        IRepository<Empresa> empresaRepository,
        IRepository<CentroCusto> centroCustoRepository)
    {
        _empresaRepository = empresaRepository;
        _centroCustoRepository = centroCustoRepository;
    }

    public async Task<EmpresaResponseDto> CadastrarEmpresaAsync(CadastroEmpresaDto dto)
    {
        var cnpjLimpo = NormalizarCnpj(dto.Cnpj);
        await ValidarCnpjUnicoAsync(cnpjLimpo, null);

        var empresa = new Empresa
        {
            EmpCnpj = cnpjLimpo,
            EmpRazaoSocial = dto.RazaoSocial.Trim(),
            EmpFantasia = dto.Fantasia.Trim()
        };

        await _empresaRepository.InserirAsync(empresa);
        await _empresaRepository.SalvarAlteracoesAsync();

        var centro = new CentroCusto { EmpId = empresa.EmpId };
        await _centroCustoRepository.InserirAsync(centro);
        await _centroCustoRepository.SalvarAlteracoesAsync();

        return Mapear(empresa, centro.CcuId);
    }

    public async Task<EmpresaResponseDto?> ObterEmpresaPorIdAsync(int id)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        if (empresa == null)
            return null;

        var centroCustoId = await ObterCentroCustoIdDaEmpresaAsync(empresa.EmpId);
        return Mapear(empresa, centroCustoId);
    }

    public async Task<IEnumerable<EmpresaResponseDto>> ListarTodasEmpresasAsync()
    {
        var empresas = await _empresaRepository.ListarTodosAsync();
        var centros = await _centroCustoRepository.ListarTodosAsync();
        var centroPorEmpresa = centros
            .GroupBy(c => c.EmpId)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CcuId).First().CcuId);

        return empresas
            .OrderBy(e => e.EmpFantasia)
            .Select(e => Mapear(e, centroPorEmpresa.GetValueOrDefault(e.EmpId)));
    }

    public async Task<EmpresaResponseDto> AtualizarEmpresaAsync(int id, CadastroEmpresaDto dto)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        if (empresa == null)
            throw new InvalidOperationException("Empresa não encontrada.");

        var cnpjLimpo = NormalizarCnpj(dto.Cnpj);
        await ValidarCnpjUnicoAsync(cnpjLimpo, id);

        empresa.EmpCnpj = cnpjLimpo;
        empresa.EmpRazaoSocial = dto.RazaoSocial.Trim();
        empresa.EmpFantasia = dto.Fantasia.Trim();

        await _empresaRepository.AtualizarAsync(empresa);
        await _empresaRepository.SalvarAlteracoesAsync();

        var centroCustoId = await ObterCentroCustoIdDaEmpresaAsync(empresa.EmpId);
        if (!centroCustoId.HasValue)
        {
            var centro = new CentroCusto { EmpId = empresa.EmpId };
            await _centroCustoRepository.InserirAsync(centro);
            await _centroCustoRepository.SalvarAlteracoesAsync();
            centroCustoId = centro.CcuId;
        }

        return Mapear(empresa, centroCustoId);
    }

    public async Task ExcluirEmpresaAsync(int id)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        if (empresa == null)
            throw new InvalidOperationException("Empresa não encontrada.");

        var centrosVinculados = (await _centroCustoRepository.BuscarTodosAsync(c => c.EmpId == id)).ToList();
        foreach (var centro in centrosVinculados)
        {
            await _centroCustoRepository.ExcluirAsync(centro);
        }

        await _centroCustoRepository.SalvarAlteracoesAsync();
        await _empresaRepository.ExcluirAsync(empresa);
        await _empresaRepository.SalvarAlteracoesAsync();
    }

    private async Task<int?> ObterCentroCustoIdDaEmpresaAsync(int empresaId)
    {
        var centros = await _centroCustoRepository.BuscarTodosAsync(c => c.EmpId == empresaId);
        return centros.OrderBy(c => c.CcuId).FirstOrDefault()?.CcuId;
    }

    private static EmpresaResponseDto Mapear(Empresa empresa, int? centroCustoId) => new()
    {
        EmpresaId = empresa.EmpId,
        CentroCustoId = centroCustoId,
        Cnpj = empresa.EmpCnpj,
        RazaoSocial = empresa.EmpRazaoSocial,
        Fantasia = empresa.EmpFantasia
    };

    private async Task ValidarCnpjUnicoAsync(string cnpj, int? idIgnorar)
    {
        var existente = await _empresaRepository.BuscarAsync(e => e.EmpCnpj == cnpj);
        if (existente != null && existente.EmpId != idIgnorar)
            throw new InvalidOperationException("Já existe uma empresa cadastrada com este CNPJ.");
    }

    private static string NormalizarCnpj(string cnpj) =>
        new string(cnpj.Where(char.IsDigit).ToArray());
}
