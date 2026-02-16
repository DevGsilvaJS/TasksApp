using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaAnotacaoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_ANC_ANOTACAO_CLIENTE",
                columns: table => new
                {
                    ANCID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIID = table.Column<int>(type: "integer", nullable: false),
                    ANCDESCRICAO = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    ANCDTCADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ANC_ANOTACAO_CLIENTE", x => x.ANCID);
                    table.ForeignKey(
                        name: "FK_TB_ANC_ANOTACAO_CLIENTE_TB_CLI_CLIENTE_CLIID",
                        column: x => x.CLIID,
                        principalTable: "TB_CLI_CLIENTE",
                        principalColumn: "CLIID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_DAS",
                columns: table => new
                {
                    DASID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DASREFERENCIA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DASDATAVENCIMENTO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DASSTATUS = table.Column<int>(type: "integer", nullable: false),
                    DASDTCADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_DAS", x => x.DASID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ANC_ANOTACAO_CLIENTE_CLIID",
                table: "TB_ANC_ANOTACAO_CLIENTE",
                column: "CLIID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_ANC_ANOTACAO_CLIENTE");

            migrationBuilder.DropTable(
                name: "TB_DAS");
        }
    }
}
