using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaCentroCustoDuplicataRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        ALTER TABLE "TB_DUP_DUPLICATA" ADD COLUMN IF NOT EXISTS "CCUID" integer NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_DUP_DUPLICATA_TB_CCU_CENTRO_CUSTO_CCUID'
            ) THEN
                ALTER TABLE "TB_DUP_DUPLICATA"
                ADD CONSTRAINT "FK_TB_DUP_DUPLICATA_TB_CCU_CENTRO_CUSTO_CCUID"
                FOREIGN KEY ("CCUID") REFERENCES "TB_CCU_CENTRO_CUSTO" ("CCUID") ON DELETE SET NULL;
            END IF;
        END $$;

        CREATE INDEX IF NOT EXISTS "IX_TB_DUP_DUPLICATA_CCUID" ON "TB_DUP_DUPLICATA" ("CCUID");
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
