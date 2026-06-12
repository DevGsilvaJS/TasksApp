using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaPlanoContasDuplicataRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        ALTER TABLE "TB_DUP_DUPLICATA" ADD COLUMN IF NOT EXISTS "PLCID" integer NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_DUP_DUPLICATA_TB_PLC_PLANO_CONTAS_PLCID'
            ) THEN
                ALTER TABLE "TB_DUP_DUPLICATA"
                ADD CONSTRAINT "FK_TB_DUP_DUPLICATA_TB_PLC_PLANO_CONTAS_PLCID"
                FOREIGN KEY ("PLCID") REFERENCES "TB_PLC_PLANO_CONTAS" ("PLCID") ON DELETE SET NULL;
            END IF;
        END $$;

        CREATE INDEX IF NOT EXISTS "IX_TB_DUP_DUPLICATA_PLCID" ON "TB_DUP_DUPLICATA" ("PLCID");
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
