using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaTituloRegimentoRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_REG_REGIMENTO'
              AND column_name = 'REGTITULO'
          ) THEN
            ALTER TABLE "TB_REG_REGIMENTO"
              ADD COLUMN "REGTITULO" character varying(300) NOT NULL DEFAULT '';

            UPDATE "TB_REG_REGIMENTO"
            SET "REGTITULO" = LEFT("REGDESCRICAO", 300)
            WHERE "REGTITULO" = '';
          END IF;
        END $$;
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
