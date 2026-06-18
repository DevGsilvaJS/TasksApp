using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirRemoverAndamentoRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        ALTER TABLE "TB_TAR_TAREFAS" DROP COLUMN IF EXISTS "TARANDAMENTO";
        DROP TABLE IF EXISTS "TB_CAD_ANDAMENTO";

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260617180000_RemoverAndamentoTarefa', '8.0.0')
        ON CONFLICT ("MigrationId") DO NOTHING;
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
