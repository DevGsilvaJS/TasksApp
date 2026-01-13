using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasDuplicataEParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_DUP_DUPLICATA",
                columns: table => new
                {
                    DUPID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DUPNUMERO = table.Column<int>(type: "integer", nullable: false),
                    DUPDATAEMISSAO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DUPNUMEROPARCELAS = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_DUP_DUPLICATA", x => x.DUPID);
                });

            migrationBuilder.CreateTable(
                name: "TB_PAR_PARCELA",
                columns: table => new
                {
                    PARID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DUPID = table.Column<int>(type: "integer", nullable: false),
                    PAR_NUMERO_PARCELA = table.Column<int>(type: "integer", nullable: false),
                    PARVALOR = table.Column<double>(type: "double precision", nullable: false),
                    PARMULTA = table.Column<double>(type: "double precision", nullable: false),
                    PARJUROS = table.Column<double>(type: "double precision", nullable: false),
                    PARVENCIMENTO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PARSTATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PAR_DATA_PAGAMENTO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PAR_PARCELA", x => x.PARID);
                    table.ForeignKey(
                        name: "FK_TB_PAR_PARCELA_TB_DUP_DUPLICATA_DUPID",
                        column: x => x.DUPID,
                        principalTable: "TB_DUP_DUPLICATA",
                        principalColumn: "DUPID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_PAR_PARCELA_DUPID",
                table: "TB_PAR_PARCELA",
                column: "DUPID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_PAR_PARCELA");

            migrationBuilder.DropTable(
                name: "TB_DUP_DUPLICATA");
        }
    }
}
