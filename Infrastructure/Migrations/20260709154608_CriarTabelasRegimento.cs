using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasRegimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_REG_REGIMENTO",
                columns: table => new
                {
                    REGID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REGSTATUS = table.Column<int>(type: "integer", nullable: false),
                    REGDESCRICAO = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REG_REGIMENTO", x => x.REGID);
                });

            migrationBuilder.CreateTable(
                name: "TB_REG_REGIMENTO_ACEITE",
                columns: table => new
                {
                    RACID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REGID = table.Column<int>(type: "integer", nullable: false),
                    USUID = table.Column<int>(type: "integer", nullable: false),
                    RACACEITO = table.Column<int>(type: "integer", nullable: false),
                    RACOBSERVACAO = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RACDATAACEITE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REG_REGIMENTO_ACEITE", x => x.RACID);
                    table.ForeignKey(
                        name: "FK_TB_REG_REGIMENTO_ACEITE_TB_REG_REGIMENTO_REGID",
                        column: x => x.REGID,
                        principalTable: "TB_REG_REGIMENTO",
                        principalColumn: "REGID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_REG_REGIMENTO_ACEITE_TB_USU_USUARIO_USUID",
                        column: x => x.USUID,
                        principalTable: "TB_USU_USUARIO",
                        principalColumn: "USUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_REG_REGIMENTO_ACEITE_REGID",
                table: "TB_REG_REGIMENTO_ACEITE",
                column: "REGID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_REG_REGIMENTO_ACEITE_USUID",
                table: "TB_REG_REGIMENTO_ACEITE",
                column: "USUID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_REG_REGIMENTO_ACEITE");

            migrationBuilder.DropTable(
                name: "TB_REG_REGIMENTO");
        }
    }
}
