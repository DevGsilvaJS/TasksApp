using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaEmpresaDuplicataParcelaRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        ALTER TABLE "TB_DUP_DUPLICATA" ADD COLUMN IF NOT EXISTS "EMPID" integer NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_DUP_DUPLICATA_TB_EMP_EMPRESA_EMPID'
            ) THEN
                ALTER TABLE "TB_DUP_DUPLICATA"
                ADD CONSTRAINT "FK_TB_DUP_DUPLICATA_TB_EMP_EMPRESA_EMPID"
                FOREIGN KEY ("EMPID") REFERENCES "TB_EMP_EMPRESA" ("EMPID") ON DELETE SET NULL;
            END IF;
        END $$;

        CREATE INDEX IF NOT EXISTS "IX_TB_DUP_DUPLICATA_EMPID" ON "TB_DUP_DUPLICATA" ("EMPID");

        ALTER TABLE "TB_PAR_PARCELA" ADD COLUMN IF NOT EXISTS "EMPID" integer NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint WHERE conname = 'FK_TB_PAR_PARCELA_TB_EMP_EMPRESA_EMPID'
            ) THEN
                ALTER TABLE "TB_PAR_PARCELA"
                ADD CONSTRAINT "FK_TB_PAR_PARCELA_TB_EMP_EMPRESA_EMPID"
                FOREIGN KEY ("EMPID") REFERENCES "TB_EMP_EMPRESA" ("EMPID") ON DELETE SET NULL;
            END IF;
        END $$;

        CREATE INDEX IF NOT EXISTS "IX_TB_PAR_PARCELA_EMPID" ON "TB_PAR_PARCELA" ("EMPID");
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
