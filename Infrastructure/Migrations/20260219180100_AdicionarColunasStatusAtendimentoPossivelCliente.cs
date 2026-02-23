using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [Migration("20260219180100_AdicionarColunasStatusAtendimentoPossivelCliente")]
    public partial class AdicionarColunasStatusAtendimentoPossivelCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: não falha se as colunas já existirem (ex.: criadas pelo Program.cs)
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_STATUS_ATENDIMENTO"" integer NULL");
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_MOTIVO_PERDA"" character varying(500) NULL");
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_DATA_STATUS_ATENDIMENTO"" timestamp with time zone NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" DROP COLUMN IF EXISTS ""POC_STATUS_ATENDIMENTO""");
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" DROP COLUMN IF EXISTS ""POC_MOTIVO_PERDA""");
            migrationBuilder.Sql(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" DROP COLUMN IF EXISTS ""POC_DATA_STATUS_ATENDIMENTO""");
        }
    }
}
