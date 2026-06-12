using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class PlanoContasService : IPlanoContasService
{
    private readonly IRepository<PlanoContas> _planoContasRepository;

    public PlanoContasService(IRepository<PlanoContas> planoContasRepository)
    {
        _planoContasRepository = planoContasRepository;
    }

    private static PlanoContasResponseDto Mapear(PlanoContas plano) => new()
    {
        PlanoContasId = plano.PlcId,
        Descricao = plano.PlcDescricao
    };

    public async Task<PlanoContasResponseDto> CadastrarPlanoContasAsync(CadastroPlanoContasDto dto)
    {
        var plano = new PlanoContas { PlcDescricao = dto.Descricao.Trim() };
        await _planoContasRepository.InserirAsync(plano);
        await _planoContasRepository.SalvarAlteracoesAsync();
        return Mapear(plano);
    }

    public async Task<PlanoContasResponseDto?> ObterPlanoContasPorIdAsync(int id)
    {
        var plano = await _planoContasRepository.GetByIdAsync(id);
        return plano == null ? null : Mapear(plano);
    }

    public async Task<IEnumerable<PlanoContasResponseDto>> ListarTodosPlanosContasAsync()
    {
        var planos = await _planoContasRepository.ListarTodosAsync();
        return planos.OrderBy(p => p.PlcDescricao).Select(Mapear);
    }

    public async Task<PlanoContasResponseDto> AtualizarPlanoContasAsync(int id, CadastroPlanoContasDto dto)
    {
        var plano = await _planoContasRepository.GetByIdAsync(id);
        if (plano == null)
            throw new InvalidOperationException("Plano de contas não encontrado.");

        plano.PlcDescricao = dto.Descricao.Trim();
        await _planoContasRepository.AtualizarAsync(plano);
        await _planoContasRepository.SalvarAlteracoesAsync();
        return Mapear(plano);
    }

    public async Task ExcluirPlanoContasAsync(int id)
    {
        var plano = await _planoContasRepository.GetByIdAsync(id);
        if (plano == null)
            throw new InvalidOperationException("Plano de contas não encontrado.");

        await _planoContasRepository.ExcluirAsync(plano);
        await _planoContasRepository.SalvarAlteracoesAsync();
    }
}
