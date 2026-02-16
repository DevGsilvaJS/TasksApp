using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DasService : IDasService
{
    private readonly IRepository<Das> _dasRepository;

    public DasService(IRepository<Das> dasRepository)
    {
        _dasRepository = dasRepository;
    }

    public async Task<DasResponseDto> CadastrarAsync(CadastroDasDto dto)
    {
        var das = new Das
        {
            DasReferencia = dto.Referencia,
            DasDataVencimento = dto.DataVencimento?.ToUniversalTime(),
            DasStatus = dto.Status,
            DasDtCadastro = DateTime.UtcNow
        };

        await _dasRepository.InserirAsync(das);
        await _dasRepository.SalvarAlteracoesAsync();

        return MontarResponseDto(das);
    }

    public async Task<DasResponseDto?> ObterPorIdAsync(int id)
    {
        var das = await _dasRepository.GetByIdAsync(id);
        return das == null ? null : MontarResponseDto(das);
    }

    public async Task<IEnumerable<DasResponseDto>> ListarTodasAsync()
    {
        var list = await _dasRepository.ListarTodosAsync();
        return list.Select(MontarResponseDto);
    }

    public async Task<DasResponseDto> AtualizarAsync(int id, CadastroDasDto dto)
    {
        var das = await _dasRepository.GetByIdAsync(id);
        if (das == null)
            throw new InvalidOperationException("DAS não encontrada.");

        das.DasReferencia = dto.Referencia;
        das.DasDataVencimento = dto.DataVencimento?.ToUniversalTime();
        das.DasStatus = dto.Status;

        await _dasRepository.AtualizarAsync(das);
        await _dasRepository.SalvarAlteracoesAsync();

        return MontarResponseDto(das);
    }

    public async Task<DasResponseDto?> AtualizarStatusAsync(int id, StatusDas status)
    {
        var das = await _dasRepository.GetByIdAsync(id);
        if (das == null)
            return null;

        das.DasStatus = status;
        await _dasRepository.AtualizarAsync(das);
        await _dasRepository.SalvarAlteracoesAsync();

        return MontarResponseDto(das);
    }

    public async Task ExcluirAsync(int id)
    {
        var das = await _dasRepository.GetByIdAsync(id);
        if (das == null)
            throw new InvalidOperationException("DAS não encontrada.");

        await _dasRepository.ExcluirAsync(das);
        await _dasRepository.SalvarAlteracoesAsync();
    }

    private static DasResponseDto MontarResponseDto(Das das)
    {
        return new DasResponseDto
        {
            DasId = das.DasId,
            Referencia = das.DasReferencia,
            DataVencimento = das.DasDataVencimento,
            Status = das.DasStatus,
            StatusDescricao = ObterDescricaoStatus(das.DasStatus),
            DataCadastro = das.DasDtCadastro
        };
    }

    private static string ObterDescricaoStatus(StatusDas status)
    {
        return status switch
        {
            StatusDas.Pendente => "Pendente",
            StatusDas.EmDia => "Em dia",
            StatusDas.Atrasado => "Atrasado",
            _ => status.ToString()
        };
    }
}
