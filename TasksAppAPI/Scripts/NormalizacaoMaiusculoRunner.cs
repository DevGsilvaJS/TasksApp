using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

/// <summary>
/// Normaliza textos existentes no banco para MAIÚSCULO (UPPER) em todas as colunas text/varchar/char.
/// Use com cuidado: essa operação atualiza o banco inteiro.
/// </summary>
public static class NormalizacaoMaiusculoRunner
{
    public static async Task ExecutarAsync(ApplicationDbContext db)
    {
        const string sql = """
        DO $$
        DECLARE
          r record;
          cmd text;
        BEGIN
          FOR r IN
            SELECT table_schema, table_name, column_name
            FROM information_schema.columns
            WHERE table_schema NOT IN ('pg_catalog','information_schema')
              AND data_type IN ('character varying','text','character')
          LOOP
            cmd := format('UPDATE %I.%I SET %I = UPPER(%I) WHERE %I IS NOT NULL;',
                          r.table_schema, r.table_name, r.column_name, r.column_name, r.column_name);
            EXECUTE cmd;
          END LOOP;
        END $$;
        """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}

