using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCentroCustoPlanoContasParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CCUID",
                table: "TB_PAR_PARCELA",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PLCID",
                table: "TB_PAR_PARCELA",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_PAR_PARCELA_CCUID",
                table: "TB_PAR_PARCELA",
                column: "CCUID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PAR_PARCELA_PLCID",
                table: "TB_PAR_PARCELA",
                column: "PLCID");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_PAR_PARCELA_TB_CCU_CENTRO_CUSTO_CCUID",
                table: "TB_PAR_PARCELA",
                column: "CCUID",
                principalTable: "TB_CCU_CENTRO_CUSTO",
                principalColumn: "CCUID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_PAR_PARCELA_TB_PLC_PLANO_CONTAS_PLCID",
                table: "TB_PAR_PARCELA",
                column: "PLCID",
                principalTable: "TB_PLC_PLANO_CONTAS",
                principalColumn: "PLCID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_PAR_PARCELA_TB_CCU_CENTRO_CUSTO_CCUID",
                table: "TB_PAR_PARCELA");

            migrationBuilder.DropForeignKey(
                name: "FK_TB_PAR_PARCELA_TB_PLC_PLANO_CONTAS_PLCID",
                table: "TB_PAR_PARCELA");

            migrationBuilder.DropIndex(
                name: "IX_TB_PAR_PARCELA_CCUID",
                table: "TB_PAR_PARCELA");

            migrationBuilder.DropIndex(
                name: "IX_TB_PAR_PARCELA_PLCID",
                table: "TB_PAR_PARCELA");

            migrationBuilder.DropColumn(
                name: "CCUID",
                table: "TB_PAR_PARCELA");

            migrationBuilder.DropColumn(
                name: "PLCID",
                table: "TB_PAR_PARCELA");
        }
    }
}
