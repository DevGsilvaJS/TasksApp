using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrocarDataEnvioPorDiaNfServicoECriarEnvioNotaServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLIDATAENVIONOTASERVICO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.AddColumn<int>(
                name: "CLIDIANFSERVICO",
                table: "TB_CLI_CLIENTE",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TB_ENS_ENVIO_NOTA_SERVICO",
                columns: table => new
                {
                    ENSID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIID = table.Column<int>(type: "integer", nullable: false),
                    ENSANO = table.Column<int>(type: "integer", nullable: false),
                    ENSMES = table.Column<int>(type: "integer", nullable: false),
                    ENSDATAENVIO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ENS_ENVIO_NOTA_SERVICO", x => x.ENSID);
                    table.ForeignKey(
                        name: "FK_TB_ENS_ENVIO_NOTA_SERVICO_TB_CLI_CLIENTE_CLIID",
                        column: x => x.CLIID,
                        principalTable: "TB_CLI_CLIENTE",
                        principalColumn: "CLIID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ENS_ENVIO_NOTA_SERVICO_CLIID",
                table: "TB_ENS_ENVIO_NOTA_SERVICO",
                column: "CLIID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_ENS_ENVIO_NOTA_SERVICO");

            migrationBuilder.DropColumn(
                name: "CLIDIANFSERVICO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.AddColumn<DateTime>(
                name: "CLIDATAENVIONOTASERVICO",
                table: "TB_CLI_CLIENTE",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
