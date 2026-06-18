using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TasksAppAPI.HostedServices;

public class EmailEnvioComercialBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailEnvioComercialBackgroundService> _logger;

    public EmailEnvioComercialBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmailEnvioComercialBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de envio de e-mails comercial iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IEmailEnvioProcessor>();
                var processou = await processor.ProcessarProximoAsync(stoppingToken);

                var delay = processou ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(5);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento em background de e-mails.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
