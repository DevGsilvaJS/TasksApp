using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaCentroCustoPlanoContasParcelaRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        ALTER TABLE "TB_PAR_PARCELA" ADD COLUMN IF NOT EXISTS "CCUID" integer NULL;
        ALTER TABLE "TB_PAR_PARCELA" ADD COLUMN IF NOT EXISTS "PLCID" integer NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_PAR_PARCELA_TB_CCU_CENTRO_CUSTO_CCUID'
            ) THEN
                ALTER TABLE "TB_PAR_PARCELA"
                ADD CONSTRAINT "FK_TB_PAR_PARCELA_TB_CCU_CENTRO_CUSTO_CCUID"
                FOREIGN KEY ("CCUID") REFERENCES "TB_CCU_CENTRO_CUSTO" ("CCUID") ON DELETE SET NULL;
            END IF;
        END $$;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_PAR_PARCELA_TB_PLC_PLANO_CONTAS_PLCID'
            ) THEN
                ALTER TABLE "TB_PAR_PARCELA"
                ADD CONSTRAINT "FK_TB_PAR_PARCELA_TB_PLC_PLANO_CONTAS_PLCID"
                FOREIGN KEY ("PLCID") REFERENCES "TB_PLC_PLANO_CONTAS" ("PLCID") ON DELETE SET NULL;
            END IF;
        END $$;

        CREATE INDEX IF NOT EXISTS "IX_TB_PAR_PARCELA_CCUID" ON "TB_PAR_PARCELA" ("CCUID");
        CREATE INDEX IF NOT EXISTS "IX_TB_PAR_PARCELA_PLCID" ON "TB_PAR_PARCELA" ("PLCID");
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
