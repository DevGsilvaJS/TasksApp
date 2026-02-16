namespace Application.DTOs;

public class PendenciasAlertasDto
{
    /// <summary>
    /// Notas de serviço pendentes de envio no mês atual (para pop-up ao logar).
    /// </summary>
    public List<NotaServicoItemDto> NotasServicoPendentesMes { get; set; } = new();

    /// <summary>
    /// Guias DAS com status Pendente ou Atrasado.
    /// </summary>
    public List<DasResponseDto> DasPendentesOuAtrasadas { get; set; } = new();
}
