using Application.Configuration;
using Application.Interfaces;
using Application.Models;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class EmailEnvioProcessor : IEmailEnvioProcessor
{
    private readonly IEmailCampanhaMemoriaStore _store;
    private readonly IEmailEnvioSmtpSender _smtpSender;
    private readonly EmailEnvioComercialOptions _opcoes;
    private readonly IAppPathsProvider _paths;
    private readonly ILogger<EmailEnvioProcessor> _logger;

    public EmailEnvioProcessor(
        IEmailCampanhaMemoriaStore store,
        IEmailEnvioSmtpSender smtpSender,
        IOptions<EmailEnvioComercialOptions> opcoes,
        IAppPathsProvider paths,
        ILogger<EmailEnvioProcessor> logger)
    {
        _store = store;
        _smtpSender = smtpSender;
        _opcoes = opcoes.Value;
        _paths = paths;
        _logger = logger;
    }

    public async Task<bool> ProcessarProximoAsync(CancellationToken cancellationToken = default)
    {
        if (_opcoes.Remetentes.Count == 0)
            return false;

        var campanha = _store.ObterCampanhaAtiva();
        if (campanha == null)
            return false;

        if (campanha.PausaAte.HasValue && campanha.PausaAte > DateTime.UtcNow)
            return false;

        if (campanha.Status == StatusCampanhaEmailComercial.Fila)
        {
            campanha.Status = StatusCampanhaEmailComercial.Processando;
            _store.Atualizar(campanha);
        }

        var proximoItem = campanha.Itens
            .Where(i => i.Status == StatusItemCampanhaEmail.Pendente)
            .OrderBy(i => i.Ordem)
            .FirstOrDefault();

        if (proximoItem == null)
        {
            FinalizarCampanha(campanha);
            return true;
        }

        var remetente = _opcoes.Remetentes[proximoItem.Ordem % _opcoes.Remetentes.Count];

        try
        {
            await _smtpSender.EnviarAsync(remetente, proximoItem.Email, campanha, cancellationToken);
            proximoItem.Status = StatusItemCampanhaEmail.Enviado;
            proximoItem.DataEnvio = DateTime.UtcNow;
            proximoItem.RemetenteEmail = remetente.Email;
            proximoItem.MensagemErro = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar para {Email}", proximoItem.Email);
            var mensagem = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            proximoItem.Status = StatusItemCampanhaEmail.Erro;
            proximoItem.DataEnvio = DateTime.UtcNow;
            proximoItem.RemetenteEmail = remetente.Email;
            proximoItem.MensagemErro = mensagem;
        }

        var aindaPendentes = campanha.Itens.Any(i => i.Status == StatusItemCampanhaEmail.Pendente);
        if (!aindaPendentes)
        {
            FinalizarCampanha(campanha);
            return true;
        }

        if ((proximoItem.Ordem + 1) % _opcoes.Remetentes.Count == 0)
            campanha.PausaAte = DateTime.UtcNow.AddSeconds(_opcoes.PausaSegundosAposParRemetentes);
        else
            campanha.PausaAte = null;

        _store.Atualizar(campanha);
        return true;
    }

    private void FinalizarCampanha(CampanhaEmailMemoria campanha)
    {
        _store.MarcarConcluida(campanha);
        LimparAnexosCampanha(campanha.Id);
    }

    private void LimparAnexosCampanha(int campanhaId)
    {
        try
        {
            var pasta = Path.Combine(_paths.ContentRootPath, _opcoes.PastaAnexosCampanha, campanhaId.ToString());
            if (Directory.Exists(pasta))
                Directory.Delete(pasta, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível remover anexos da campanha {CampanhaId}", campanhaId);
        }
    }
}
