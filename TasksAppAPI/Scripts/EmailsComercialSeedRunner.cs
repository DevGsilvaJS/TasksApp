using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

/// <summary>
/// Importa destinatários de e-mail comercial a partir de arquivo texto (uma ou mais linhas com ;).
/// </summary>
public static class EmailsComercialSeedRunner
{
    private const string NomeArquivo = "emails-comercial-raw.txt";

    public static async Task ExecutarAsync(ApplicationDbContext db, string scriptsFolderPath)
    {
        var caminho = Path.Combine(Path.GetFullPath(scriptsFolderPath), "Data", NomeArquivo);
        if (!File.Exists(caminho))
        {
            Console.WriteLine($"[E-mails comercial] Arquivo não encontrado: {caminho}. Ignorando seed.");
            return;
        }

        var linhas = await File.ReadAllLinesAsync(caminho);
        var emails = ExtrairEmailsUnicos(linhas);
        if (emails.Count == 0)
        {
            Console.WriteLine("[E-mails comercial] Nenhum e-mail válido no arquivo.");
            return;
        }

        var existentes = await db.EmailsEnvioComercial
            .Select(e => e.EecEmail)
            .ToListAsync();
        var setExistentes = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);

        var novos = emails
            .Where(e => !setExistentes.Contains(e))
            .Select(email => new EmailEnvioComercial
            {
                EecEmail = email,
                EecNaoEnviar = false,
                EecDataCadastro = DateTime.UtcNow
            })
            .ToList();

        if (novos.Count == 0)
        {
            Console.WriteLine($"[E-mails comercial] Todos os {emails.Count} e-mail(s) já estão cadastrados.");
            return;
        }

        db.EmailsEnvioComercial.AddRange(novos);
        await db.SaveChangesAsync();
        Console.WriteLine($"[E-mails comercial] {novos.Count} e-mail(s) inserido(s). Total único no arquivo: {emails.Count}.");
    }

    internal static HashSet<string> ExtrairEmailsUnicos(IEnumerable<string> linhas)
    {
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var linha in linhas)
        {
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            var partes = linha.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var parte in partes)
            {
                var email = NormalizarEmail(parte);
                if (!string.IsNullOrWhiteSpace(email))
                    emails.Add(email);
            }
        }

        return emails;
    }

    private static string NormalizarEmail(string valor)
    {
        var limpo = valor.Trim().Trim('"').Trim();
        limpo = limpo.Replace('\t', ' ').Trim();
        return limpo;
    }
}
