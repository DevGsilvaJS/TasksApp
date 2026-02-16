using Application.DTOs;

namespace Application.Interfaces;

public interface INotaServicoService
{
    /// <summary>
    /// Lista todas as notas de serviço do mês (pendentes e enviadas) para o card do dashboard.
    /// Clientes com DiaNfServico preenchido; para o mês/ano, cria ou retorna registro de envio.
    /// </summary>
    Task<List<NotaServicoItemDto>> ListarNotasDoMesAsync(int ano, int mes);

    /// <summary>
    /// Marca a nota de serviço do cliente no mês como enviada (e opcionalmente a data de envio).
    /// </summary>
    Task<NotaServicoItemDto?> MarcarComoEnviadoAsync(int clienteId, int ano, int mes, DateTime? dataEnvio = null);

    /// <summary>
    /// Retorna apenas as notas pendentes de envio no mês (para pop-up de alerta).
    /// </summary>
    Task<List<NotaServicoItemDto>> ListarPendentesDoMesAsync(int ano, int mes);
}
