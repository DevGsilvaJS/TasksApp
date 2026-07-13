using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaRegimentoAceiteLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_REG_REGIMENTO_ACEITE_LOG",
                columns: table => new
                {
                    RALID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REGID = table.Column<int>(type: "integer", nullable: false),
                    USUID = table.Column<int>(type: "integer", nullable: false),
                    RALTIPO = table.Column<int>(type: "integer", nullable: false),
                    RALDECISAO = table.Column<int>(type: "integer", nullable: true),
                    RALOBSERVACAO = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RALDATA = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REG_REGIMENTO_ACEITE_LOG", x => x.RALID);
                    table.ForeignKey(
                        name: "FK_TB_REG_REGIMENTO_ACEITE_LOG_TB_REG_REGIMENTO_REGID",
                        column: x => x.REGID,
                        principalTable: "TB_REG_REGIMENTO",
                        principalColumn: "REGID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_REG_REGIMENTO_ACEITE_LOG_TB_USU_USUARIO_USUID",
                        column: x => x.USUID,
                        principalTable: "TB_USU_USUARIO",
                        principalColumn: "USUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_REG_REGIMENTO_ACEITE_LOG_REGID",
                table: "TB_REG_REGIMENTO_ACEITE_LOG",
                column: "REGID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_REG_REGIMENTO_ACEITE_LOG_USUID",
                table: "TB_REG_REGIMENTO_ACEITE_LOG",
                column: "USUID");

            migrationBuilder.Sql("""
                INSERT INTO "TB_REG_REGIMENTO_ACEITE_LOG" ("REGID", "USUID", "RALTIPO", "RALDECISAO", "RALOBSERVACAO", "RALDATA")
                SELECT "REGID",
                       "USUID",
                       CASE WHEN "RACACEITO" = 0 THEN 1 ELSE 2 END,
                       "RACACEITO",
                       "RACOBSERVACAO",
                       COALESCE("RACDATAACEITE", NOW() AT TIME ZONE 'UTC')
                FROM "TB_REG_REGIMENTO_ACEITE";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_REG_REGIMENTO_ACEITE_LOG");
        }
    }
}
