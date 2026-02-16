using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AlertasService : IAlertasService
{
    private readonly IRepository<Das> _dasRepository;
    private readonly INotaServicoService _notaServicoService;

    public AlertasService(
        IRepository<Das> dasRepository,
        INotaServicoService notaServicoService)
    {
        _dasRepository = dasRepository;
        _notaServicoService = notaServicoService;
    }

    public async Task<PendenciasAlertasDto> ObterPendenciasAsync(int diasParaAlertaNota = 30)
    {
        var now = DateTime.UtcNow;
        var notasPendentes = await _notaServicoService.ListarPendentesDoMesAsync(now.Year, now.Month);

        var todasDas = await _dasRepository.ListarTodosAsync();
        var dasPendentesOuAtrasadas = todasDas
            .Where(d => d.DasStatus == StatusDas.Pendente || d.DasStatus == StatusDas.Atrasado)
            .Select(d => new DasResponseDto
            {
                DasId = d.DasId,
                Referencia = d.DasReferencia,
                DataVencimento = d.DasDataVencimento,
                Status = d.DasStatus,
                StatusDescricao = d.DasStatus == StatusDas.Pendente ? "Pendente" : d.DasStatus == StatusDas.Atrasado ? "Atrasado" : "Em dia",
                DataCadastro = d.DasDtCadastro
            })
            .ToList();

        return new PendenciasAlertasDto
        {
            NotasServicoPendentesMes = notasPendentes,
            DasPendentesOuAtrasadas = dasPendentesOuAtrasadas
        };
    }
}
