using ClosedXML.Excel;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

/// <summary>
/// Na inicialização do sistema, lê a planilha Excel na pasta Planilha e popula a tabela de possíveis clientes.
/// Colunas: C=Código, D=Loja, E=Status, F=Fantasia, I=DDD, Q=CNPJ, R=Razão Social,
/// AA=E-mail comercial, AC=Cel DDD, AD=Celular.
/// </summary>
public static class PlanilhaPossiveisClientesRunner
{
    private const int ColCodigo = 3;       // C
    private const int ColLoja = 4;         // D
    private const int ColStatus = 5;       // E
    private const int ColFantasia = 6;     // F
    private const int ColDdd = 9;          // I
    private const int ColCnpj = 17;        // Q
    private const int ColRazaoSocial = 18; // R
    private const int ColEmailComercial = 27; // AA
    private const int ColCelDdd = 29;      // AC
    private const int ColCelular = 30;     // AD
    private const int FirstDataRow = 2;    // linha 1 = cabeçalho

    public static async Task ExecutarAsync(ApplicationDbContext db, string planilhaFolderPath)
    {
        var dir = Path.GetFullPath(planilhaFolderPath);
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"[Planilha] Pasta não encontrada: {dir}. Ignorando importação de possíveis clientes.");
            return;
        }

        var arquivos = Directory.GetFiles(dir, "*.xlsx", SearchOption.TopDirectoryOnly);
        if (arquivos.Length == 0)
        {
            Console.WriteLine($"[Planilha] Nenhum arquivo .xlsx em {dir}. Ignorando importação.");
            return;
        }

        var arquivo = arquivos[0];
        Console.WriteLine($"[Planilha] Lendo {Path.GetFileName(arquivo)}...");

        using var workbook = new XLWorkbook(arquivo);
        var sheet = workbook.Worksheets.First();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow < FirstDataRow)
        {
            Console.WriteLine("[Planilha] Nenhuma linha de dados na planilha.");
            return;
        }

        // Chave = Código + Loja (mesmo código pode repetir para várias filiais)
        var existentes = await db.PossiveisClientes.ToListAsync();
        var existentesPorCodigoELoja = existentes.ToDictionary(p => (p.PocCodigo, p.PocLoja ?? ""));
        var atualizados = 0;
        var inseridos = 0;

        for (var row = FirstDataRow; row <= lastRow; row++)
        {
            var codigo = ObterValor(sheet, row, ColCodigo);
            if (string.IsNullOrWhiteSpace(codigo))
                continue;

            codigo = codigo.Trim();
            var loja = NullIfEmpty(ObterValor(sheet, row, ColLoja)) ?? "";
            var status = ObterValor(sheet, row, ColStatus);
            var fantasia = ObterValor(sheet, row, ColFantasia);
            var ddd = ObterValor(sheet, row, ColDdd);
            var cnpj = ObterValor(sheet, row, ColCnpj);
            var razaoSocial = ObterValor(sheet, row, ColRazaoSocial);
            var emailComercial = ObterValor(sheet, row, ColEmailComercial);
            var celDdd = ObterValor(sheet, row, ColCelDdd);
            var celular = ObterValor(sheet, row, ColCelular);

            if (existentesPorCodigoELoja.TryGetValue((codigo, loja), out var existente))
            {
                existente.PocLoja = string.IsNullOrEmpty(loja) ? null : loja;
                existente.PocStatus = NullIfEmpty(status);
                existente.PocFantasia = NullIfEmpty(fantasia);
                existente.PocDdd = NullIfEmpty(ddd);
                existente.PocCnpj = NullIfEmpty(cnpj);
                existente.PocRazaoSocial = NullIfEmpty(razaoSocial);
                existente.PocEmailComercial = NullIfEmpty(emailComercial);
                existente.PocCelDdd = NullIfEmpty(celDdd);
                existente.PocCelular = NullIfEmpty(celular);
                existente.PocDataImportacao = DateTime.UtcNow;
                atualizados++;
            }
            else
            {
                var novo = new PossivelCliente
                {
                    PocCodigo = codigo,
                    PocLoja = string.IsNullOrEmpty(loja) ? null : loja,
                    PocStatus = NullIfEmpty(status),
                    PocFantasia = NullIfEmpty(fantasia),
                    PocDdd = NullIfEmpty(ddd),
                    PocCnpj = NullIfEmpty(cnpj),
                    PocRazaoSocial = NullIfEmpty(razaoSocial),
                    PocEmailComercial = NullIfEmpty(emailComercial),
                    PocCelDdd = NullIfEmpty(celDdd),
                    PocCelular = NullIfEmpty(celular),
                    PocDataImportacao = DateTime.UtcNow
                };
                db.PossiveisClientes.Add(novo);
                existentesPorCodigoELoja[(codigo, loja)] = novo;
                inseridos++;
            }
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"[Planilha] Possíveis clientes: {inseridos} inseridos, {atualizados} atualizados.");
    }

    private static string? ObterValor(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty())
            return null;
        var v = cell.GetString();
        if (!string.IsNullOrWhiteSpace(v))
            return v.Trim();
        if (cell.TryGetValue(out double d))
            return d.ToString("G");
        var val = cell.Value;
        return val.ToString()?.Trim();
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
