using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

/// <summary>
/// Executa uma vez na inicialização: vincula duplicatas CR (Contas a Receber) sem cliente
/// aos clientes da lista, quando a descrição da duplicata contém o código do cliente.
/// Lista: descrição (nome fantasia) = código entre parênteses (CliCodigo).
/// </summary>
public static class AtualizacaoCrClientesRunner
{
    /// <summary>Descrição e código do cliente (código entre parênteses na lista do usuário).</summary>
    private static readonly (string Descricao, string Codigo)[] ClientesParaAtualizarCr =
    {
        ("DI OCCHIO", "2494"),
        ("PARIS VISION", "2494"),   // 02494 = 2494
        ("MAIS VISÃO", "4597"),
        ("SANTA VISTA", "3844"),
        ("GAZETTA", "3552"),
        ("VISÃO DE TODOS", "4146"),
        ("RENDENTORA", "807"),
        ("SAO JOSÉ", "4224"),
    };

    /// <summary>Códigos distintos para processar (evita processar 2494 duas vezes).</summary>
    private static readonly string[] CodigosDistintos = ClientesParaAtualizarCr
        .Select(x => x.Codigo)
        .Distinct()
        .ToArray();

    public static async Task ExecutarUmaVezAsync(ApplicationDbContext db)
    {
        foreach (var codigo in CodigosDistintos)
        {
            var cliente = await db.Clientes
                .FirstOrDefaultAsync(c => c.CliCodigo == codigo);
            if (cliente == null)
            {
                Console.WriteLine($"[CR] Cliente com código {codigo} não encontrado; ignorando.");
                continue;
            }

            // Duplicatas CR sem cliente e cuja descrição contém o código do cliente
            var duplicatas = await db.Duplicatas
                .Where(d => d.DupTipo == "CR"
                    && d.CliId == null
                    && d.DupDescricaoDespesa != null
                    && (d.DupDescricaoDespesa.Contains(codigo)
                        || (codigo == "2494" && d.DupDescricaoDespesa.Contains("02494"))))
                .ToListAsync();

            if (duplicatas.Count == 0)
                continue;

            foreach (var dup in duplicatas)
                dup.CliId = cliente.CliId;

            await db.SaveChangesAsync();
            Console.WriteLine($"[CR] Atualizadas {duplicatas.Count} duplicata(s) CR para cliente código {codigo} (CliId={cliente.CliId}).");
        }
    }
}
