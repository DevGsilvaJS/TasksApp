using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaClienteContratoValor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_CAD_STATUS_ATEND_COMERCIAL",
                columns: table => new
                {
                    SACID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SACNUMERO = table.Column<int>(type: "integer", nullable: false),
                    SACDESCRICAO = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SACATIVO = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_STATUS_ATEND_COMERCIAL", x => x.SACID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLI_CONTRATO_VALOR",
                columns: table => new
                {
                    CVCID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIID = table.Column<int>(type: "integer", nullable: false),
                    CVCVALORMENSAL = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CVCDATAINICIO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CVCDATAFIM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLI_CONTRATO_VALOR", x => x.CVCID);
                    table.ForeignKey(
                        name: "FK_TB_CLI_CONTRATO_VALOR_TB_CLI_CLIENTE_CLIID",
                        column: x => x.CLIID,
                        principalTable: "TB_CLI_CLIENTE",
                        principalColumn: "CLIID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLI_CONTRATO_VALOR_CLIID",
                table: "TB_CLI_CONTRATO_VALOR",
                column: "CLIID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CAD_STATUS_ATEND_COMERCIAL");

            migrationBuilder.DropTable(
                name: "TB_CLI_CONTRATO_VALOR");
        }
    }
}
