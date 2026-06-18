using Application.Configuration;
using Application.Models;

namespace Application.Interfaces;

public interface IEmailEnvioSmtpSender
{
    Task EnviarAsync(
        RemetenteEmailOptions remetente,
        string emailDestinatario,
        CampanhaEmailMemoria campanha,
        CancellationToken cancellationToken = default);
}

public interface IEmailEnvioProcessor
{
    Task<bool> ProcessarProximoAsync(CancellationToken cancellationToken = default);
}
