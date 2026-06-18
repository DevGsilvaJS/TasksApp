using Application.Configuration;
using Application.Interfaces;
using Application.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Application.Services;

public class EmailEnvioSmtpSender : IEmailEnvioSmtpSender
{
    private const string ContentIdAssinatura = "assinatura-remetente";

    private readonly EmailEnvioComercialOptions _opcoes;
    private readonly IAppPathsProvider _paths;
    private readonly ILogger<EmailEnvioSmtpSender> _logger;

    public EmailEnvioSmtpSender(
        IOptions<EmailEnvioComercialOptions> opcoes,
        IAppPathsProvider paths,
        ILogger<EmailEnvioSmtpSender> logger)
    {
        _opcoes = opcoes.Value;
        _paths = paths;
        _logger = logger;
    }

    public async Task EnviarAsync(
        RemetenteEmailOptions remetente,
        string emailDestinatario,
        CampanhaEmailMemoria campanha,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(remetente.Senha))
            throw new InvalidOperationException($"Senha SMTP não configurada para {remetente.Email}.");

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(remetente.Nome, remetente.Email));
        mensagem.To.Add(new MailboxAddress("Destinatário", emailDestinatario));
        mensagem.Subject = campanha.Assunto;

        var corpo = MontarCorpoEmail(remetente, campanha);
        AdicionarAnexosCampanha(corpo, campanha.Id);
        mensagem.Body = corpo.ToMessageBody();

        using var cliente = new SmtpClient();
        await cliente.ConnectAsync(_opcoes.SmtpHost, _opcoes.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await cliente.AuthenticateAsync(remetente.Email, remetente.Senha, cancellationToken);
        await cliente.SendAsync(mensagem, cancellationToken);
        await cliente.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("E-mail enviado para {Destinatario} via {Remetente}", emailDestinatario, remetente.Email);
    }

    private BodyBuilder MontarCorpoEmail(RemetenteEmailOptions remetente, CampanhaEmailMemoria campanha)
    {
        var corpo = new BodyBuilder();
        var htmlCorpo = campanha.CorpoHtml;
        var caminhoAssinatura = ResolverCaminhoAssinatura(remetente);

        if (caminhoAssinatura != null)
        {
            var recurso = corpo.LinkedResources.Add(caminhoAssinatura);
            recurso.ContentId = ContentIdAssinatura;
            htmlCorpo += $"""
                <br/>
                <p style="margin-top:1em;">
                  <img src="cid:{ContentIdAssinatura}" alt="Assinatura" style="max-width:320px;height:auto;" />
                </p>
                """;
        }
        else if (!string.IsNullOrWhiteSpace(remetente.AssinaturaArquivo))
        {
            _logger.LogWarning(
                "Arquivo de assinatura não encontrado para {Remetente}: {Arquivo}",
                remetente.Email,
                remetente.AssinaturaArquivo);
        }

        corpo.HtmlBody = htmlCorpo;
        return corpo;
    }

    private string? ResolverCaminhoAssinatura(RemetenteEmailOptions remetente)
    {
        if (string.IsNullOrWhiteSpace(remetente.AssinaturaArquivo))
            return null;

        var pasta = ResolverPastaAssinaturas();
        var caminho = Path.Combine(pasta, remetente.AssinaturaArquivo);
        return File.Exists(caminho) ? caminho : null;
    }

    private string ResolverPastaAssinaturas()
    {
        var baseDir = _paths.ContentRootPath;
        var nomePasta = _opcoes.PastaAssinaturas;

        foreach (var pasta in new[]
        {
            Path.Combine(AppContext.BaseDirectory, nomePasta),
            Path.Combine(baseDir, nomePasta),
            Path.GetFullPath(Path.Combine(baseDir, "..", nomePasta))
        })
        {
            if (Directory.Exists(pasta))
                return pasta;
        }

        return Path.Combine(baseDir, nomePasta);
    }

    private void AdicionarAnexosCampanha(BodyBuilder corpo, int campanhaId)
    {
        var pasta = Path.Combine(_paths.ContentRootPath, _opcoes.PastaAnexosCampanha, campanhaId.ToString());
        if (!Directory.Exists(pasta)) return;

        foreach (var arquivo in Directory.GetFiles(pasta))
            corpo.Attachments.Add(arquivo);
    }
}
