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

    private static EmpresaResponseDto Mapear(Empresa empresa) => new()
    {
        EmpresaId = empresa.EmpId,
        Cnpj = empresa.EmpCnpj,
        RazaoSocial = empresa.EmpRazaoSocial,
        Fantasia = empresa.EmpFantasia
    };

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
        return Mapear(empresa);
    }

    public async Task<EmpresaResponseDto?> ObterEmpresaPorIdAsync(int id)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        return empresa == null ? null : Mapear(empresa);
    }

    public async Task<IEnumerable<EmpresaResponseDto>> ListarTodasEmpresasAsync()
    {
        var empresas = await _empresaRepository.ListarTodosAsync();
        return empresas.OrderBy(e => e.EmpFantasia).Select(Mapear);
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
        return Mapear(empresa);
    }

    public async Task ExcluirEmpresaAsync(int id)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        if (empresa == null)
            throw new InvalidOperationException("Empresa não encontrada.");

        var centrosVinculados = await _centroCustoRepository.BuscarTodosAsync(c => c.EmpId == id);
        if (centrosVinculados.Any())
            throw new InvalidOperationException("Não é possível excluir: existem centros de custo vinculados a esta empresa.");

        await _empresaRepository.ExcluirAsync(empresa);
        await _empresaRepository.SalvarAlteracoesAsync();
    }

    private async Task ValidarCnpjUnicoAsync(string cnpj, int? idIgnorar)
    {
        var existente = await _empresaRepository.BuscarAsync(e => e.EmpCnpj == cnpj);
        if (existente != null && existente.EmpId != idIgnorar)
            throw new InvalidOperationException("Já existe uma empresa cadastrada com este CNPJ.");
    }

    private static string NormalizarCnpj(string cnpj) =>
        new string(cnpj.Where(char.IsDigit).ToArray());
}
