using Application.DTOs;

namespace Application.Interfaces;

public interface IAlertasService
{
    /// <summary>
    /// Retorna pendências para exibição em pop-up: clientes que precisam enviar nota de serviços e DAS pendentes/atrasadas.
    /// </summary>
    /// <param name="diasParaAlertaNota">Alertar se a última nota foi enviada há mais de X dias (ou nunca). Padrão: 30.</param>
    Task<PendenciasAlertasDto> ObterPendenciasAsync(int diasParaAlertaNota = 30);
}
