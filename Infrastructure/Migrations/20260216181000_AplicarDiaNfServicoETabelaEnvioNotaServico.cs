using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Aplica alterações de DiaNfServico e TB_ENS via SQL idempotente.
    /// Use quando a migration anterior foi aplicada vazia e não há acesso ao BD para corrigir manualmente.
    /// </summary>
    public partial class AplicarDiaNfServicoETabelaEnvioNotaServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Remover coluna antiga (se existir)
            migrationBuilder.Sql(@"
                ALTER TABLE ""TB_CLI_CLIENTE"" DROP COLUMN IF EXISTS ""CLIDATAENVIONOTASERVICO"";
            ");

            // 2) Adicionar CLIDIANFSERVICO (se não existir)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'TB_CLI_CLIENTE'
                          AND column_name = 'CLIDIANFSERVICO'
                    ) THEN
                        ALTER TABLE ""TB_CLI_CLIENTE"" ADD COLUMN ""CLIDIANFSERVICO"" integer NULL;
                    END IF;
                END $$;
            ");

            // 3) Criar tabela TB_ENS_ENVIO_NOTA_SERVICO (se não existir)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TB_ENS_ENVIO_NOTA_SERVICO"" (
                    ""ENSID"" serial PRIMARY KEY,
                    ""CLIID"" integer NOT NULL,
                    ""ENSANO"" integer NOT NULL,
                    ""ENSMES"" integer NOT NULL,
                    ""ENSDATAENVIO"" timestamp with time zone NULL
                );
            ");

            // 4) Criar índice (se não existir)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_TB_ENS_ENVIO_NOTA_SERVICO_CLIID""
                ON ""TB_ENS_ENVIO_NOTA_SERVICO"" (""CLIID"");
            ");

            // 5) Adicionar FK (se não existir)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_TB_ENS_ENVIO_NOTA_SERVICO_TB_CLI_CLIENTE_CLIID'
                    ) THEN
                        ALTER TABLE ""TB_ENS_ENVIO_NOTA_SERVICO""
                        ADD CONSTRAINT ""FK_TB_ENS_ENVIO_NOTA_SERVICO_TB_CLI_CLIENTE_CLIID""
                        FOREIGN KEY (""CLIID"") REFERENCES ""TB_CLI_CLIENTE"" (""CLIID"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""TB_ENS_ENVIO_NOTA_SERVICO"" DROP CONSTRAINT IF EXISTS ""FK_TB_ENS_ENVIO_NOTA_SERVICO_TB_CLI_CLIENTE_CLIID"";
                DROP TABLE IF EXISTS ""TB_ENS_ENVIO_NOTA_SERVICO"";
                ALTER TABLE ""TB_CLI_CLIENTE"" DROP COLUMN IF EXISTS ""CLIDIANFSERVICO"";
                ALTER TABLE ""TB_CLI_CLIENTE"" ADD COLUMN ""CLIDATAENVIONOTASERVICO"" timestamp with time zone NULL;
            ");
        }
    }
}
