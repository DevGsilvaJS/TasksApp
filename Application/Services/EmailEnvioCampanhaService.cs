using Application.Configuration;
using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class EmailEnvioCampanhaService : IEmailEnvioCampanhaService
{
    private readonly IEmailCampanhaMemoriaStore _store;
    private readonly EmailEnvioComercialOptions _opcoes;
    private readonly IAppPathsProvider _paths;

    public EmailEnvioCampanhaService(
        IEmailCampanhaMemoriaStore store,
        IOptions<EmailEnvioComercialOptions> opcoes,
        IAppPathsProvider paths)
    {
        _store = store;
        _opcoes = opcoes.Value;
        _paths = paths;
    }

    public async Task<EnfileirarCampanhaEmailResponseDto> EnfileirarCampanhaAsync(
        string assunto,
        string corpoHtml,
        IEnumerable<string> emailsDestinatarios,
        IEnumerable<IFormFile>? anexos)
    {
        if (_store.ObterCampanhaAtiva() != null)
            throw new InvalidOperationException("Já existe uma campanha em andamento. Aguarde a conclusão.");

        var listaEmails = emailsDestinatarios
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToList();

        if (listaEmails.Count == 0)
            throw new InvalidOperationException("Nenhum destinatário informado.");

        ValidarConfiguracaoSmtp();

        var campanha = new CampanhaEmailMemoria
        {
            Assunto = EmailConteudoNormalizador.NormalizarAssunto(assunto),
            CorpoHtml = EmailConteudoNormalizador.NormalizarCorpoHtml(corpoHtml),
            Itens = listaEmails.Select((email, indice) => new ItemCampanhaEmailMemoria
            {
                Email = email,
                Ordem = indice,
                Status = StatusItemCampanhaEmail.Pendente
            }).ToList()
        };

        _store.Criar(campanha);
        await SalvarAnexosCampanhaAsync(campanha.Id, anexos);

        return new EnfileirarCampanhaEmailResponseDto
        {
            CampanhaId = campanha.Id,
            TotalDestinatarios = listaEmails.Count,
            Mensagem = "Campanha enfileirada. O envio ocorrerá em segundo plano."
        };
    }

    public Task<CampanhaEmailStatusResponseDto?> ObterStatusCampanhaAsync(int campanhaId)
    {
        var campanha = _store.ObterPorId(campanhaId);
        return Task.FromResult(campanha == null ? null : MapearStatus(campanha));
    }

    public Task<CampanhaEmailStatusResponseDto?> ObterCampanhaAtivaAsync()
    {
        var campanha = _store.ObterCampanhaAtiva();
        return Task.FromResult(campanha == null ? null : MapearStatus(campanha));
    }

    public Task<RelatorioCampanhaEmailResponseDto?> ObterRelatorioAsync(int campanhaId)
    {
        var campanha = _store.ObterPorId(campanhaId);
        if (campanha == null || campanha.Status != StatusCampanhaEmailComercial.Concluida)
            return Task.FromResult<RelatorioCampanhaEmailResponseDto?>(null);

        return Task.FromResult<RelatorioCampanhaEmailResponseDto?>(MapearRelatorio(campanha));
    }

    public Task<IReadOnlyList<RelatorioCampanhaEmailResponseDto>> ListarRelatoriosAsync()
    {
        var relatorios = _store.ListarHistorico()
            .Where(c => c.Status == StatusCampanhaEmailComercial.Concluida)
            .Select(MapearRelatorio)
            .ToList();

        return Task.FromResult<IReadOnlyList<RelatorioCampanhaEmailResponseDto>>(relatorios);
    }

    private static CampanhaEmailStatusResponseDto MapearStatus(CampanhaEmailMemoria campanha)
    {
        var enviados = campanha.Itens.Count(i => i.Status == StatusItemCampanhaEmail.Enviado);
        var erros = campanha.Itens.Count(i => i.Status == StatusItemCampanhaEmail.Erro);
        var pendentes = campanha.Itens.Count(i => i.Status == StatusItemCampanhaEmail.Pendente);

        return new CampanhaEmailStatusResponseDto
        {
            Id = campanha.Id,
            Status = campanha.Status.ToString(),
            Assunto = campanha.Assunto,
            TotalItens = campanha.Itens.Count,
            Enviados = enviados,
            Erros = erros,
            Pendentes = pendentes,
            DataCriacao = campanha.DataCriacao,
            PausaAte = campanha.PausaAte
        };
    }

    private static RelatorioCampanhaEmailResponseDto MapearRelatorio(CampanhaEmailMemoria campanha)
    {
        var enviados = campanha.Itens
            .Where(i => i.Status == StatusItemCampanhaEmail.Enviado)
            .Select(MapearItemRelatorio)
            .ToList();

        var erros = campanha.Itens
            .Where(i => i.Status == StatusItemCampanhaEmail.Erro)
            .Select(MapearItemRelatorio)
            .ToList();

        return new RelatorioCampanhaEmailResponseDto
        {
            Id = campanha.Id,
            Assunto = campanha.Assunto,
            Status = campanha.Status.ToString(),
            DataCriacao = campanha.DataCriacao,
            DataConclusao = campanha.DataConclusao,
            TotalItens = campanha.Itens.Count,
            Enviados = enviados.Count,
            Erros = erros.Count,
            ItensEnviados = enviados,
            ItensComErro = erros
        };
    }

    private static RelatorioItemEmailDto MapearItemRelatorio(ItemCampanhaEmailMemoria item) =>
        new()
        {
            Email = item.Email,
            RemetenteEmail = item.RemetenteEmail,
            DataEnvio = item.DataEnvio,
            MensagemErro = item.MensagemErro
        };

    private void ValidarConfiguracaoSmtp()
    {
        if (_opcoes.Remetentes.Count == 0)
            throw new InvalidOperationException("Nenhum remetente SMTP configurado em EmailEnvioComercial:Remetentes.");

        var semSenha = _opcoes.Remetentes
            .Where(r => string.IsNullOrWhiteSpace(r.Senha))
            .Select(r => r.Email)
            .ToList();

        if (semSenha.Count > 0)
        {
            throw new InvalidOperationException(
                $"Senha SMTP não configurada para: {string.Join(", ", semSenha)}. " +
                "Configure via User Secrets (desenvolvimento) ou variáveis de ambiente " +
                "EmailEnvioComercial__Remetentes__0__Senha e EmailEnvioComercial__Remetentes__1__Senha.");
        }
    }

    private async Task SalvarAnexosCampanhaAsync(int campanhaId, IEnumerable<IFormFile>? anexos)
    {
        if (anexos == null) return;

        var pasta = Path.Combine(_paths.ContentRootPath, _opcoes.PastaAnexosCampanha, campanhaId.ToString());
        Directory.CreateDirectory(pasta);

        var indice = 0;
        foreach (var arquivo in anexos.Where(a => a.Length > 0))
        {
            var nome = $"anexo_{indice}{Path.GetExtension(arquivo.FileName)}";
            var caminho = Path.Combine(pasta, nome);
            await using var stream = File.Create(caminho);
            await arquivo.CopyToAsync(stream);
            indice++;
        }
    }
}
