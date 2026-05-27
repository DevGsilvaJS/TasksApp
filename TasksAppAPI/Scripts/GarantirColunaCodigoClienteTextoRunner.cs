using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

public static class GarantirColunaCodigoClienteTextoRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = @"
DO $$
BEGIN
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
      AND table_name = 'TB_CLI_CLIENTE'
      AND column_name = 'CLICODIGO'
      AND data_type IN ('integer', 'bigint', 'smallint', 'numeric')
  ) THEN
    ALTER TABLE ""TB_CLI_CLIENTE""
      ALTER COLUMN ""CLICODIGO"" TYPE varchar(20)
      USING ""CLICODIGO""::text;
  END IF;
EXCEPTION
  WHEN others THEN
    -- Evita quebrar startup caso o banco esteja em estado inesperado.
    NULL;
END $$;
";

        try
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
        catch
        {
            // Se falhar (ex.: banco sem permissão), não interrompe a API.
        }
    }
}

