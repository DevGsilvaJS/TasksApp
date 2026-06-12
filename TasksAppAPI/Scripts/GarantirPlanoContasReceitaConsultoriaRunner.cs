using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirPlanoContasReceitaConsultoriaRunner
{
    public const string DescricaoPlano = "RECEITA DE CONSULTORIA";

    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        INSERT INTO "TB_PLC_PLANO_CONTAS" ("PLCDESCRICAO")
        SELECT 'RECEITA DE CONSULTORIA'
        WHERE NOT EXISTS (
            SELECT 1 FROM "TB_PLC_PLANO_CONTAS"
            WHERE UPPER(TRIM("PLCDESCRICAO")) = 'RECEITA DE CONSULTORIA'
        );
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
