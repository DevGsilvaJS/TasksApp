using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasEmpresaCentroCustoPlanoContas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_EMP_EMPRESA",
                columns: table => new
                {
                    EMPID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMPCNPJ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EMPRAZAOSOCIAL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EMPFANTASIA = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_EMP_EMPRESA", x => x.EMPID);
                });

            migrationBuilder.CreateTable(
                name: "TB_PLC_PLANO_CONTAS",
                columns: table => new
                {
                    PLCID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PLCDESCRICAO = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PLC_PLANO_CONTAS", x => x.PLCID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CCU_CENTRO_CUSTO",
                columns: table => new
                {
                    CCUID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMPID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CCU_CENTRO_CUSTO", x => x.CCUID);
                    table.ForeignKey(
                        name: "FK_TB_CCU_CENTRO_CUSTO_TB_EMP_EMPRESA_EMPID",
                        column: x => x.EMPID,
                        principalTable: "TB_EMP_EMPRESA",
                        principalColumn: "EMPID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CCU_CENTRO_CUSTO_EMPID",
                table: "TB_CCU_CENTRO_CUSTO",
                column: "EMPID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CCU_CENTRO_CUSTO");

            migrationBuilder.DropTable(
                name: "TB_PLC_PLANO_CONTAS");

            migrationBuilder.DropTable(
                name: "TB_EMP_EMPRESA");
        }
    }
}
