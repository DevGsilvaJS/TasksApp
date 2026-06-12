using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCentroCustoDuplicata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CCUID",
                table: "TB_DUP_DUPLICATA",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_DUP_DUPLICATA_CCUID",
                table: "TB_DUP_DUPLICATA",
                column: "CCUID");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_CCU_CENTRO_CUSTO_CCUID",
                table: "TB_DUP_DUPLICATA",
                column: "CCUID",
                principalTable: "TB_CCU_CENTRO_CUSTO",
                principalColumn: "CCUID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_CCU_CENTRO_CUSTO_CCUID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropIndex(
                name: "IX_TB_DUP_DUPLICATA_CCUID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropColumn(
                name: "CCUID",
                table: "TB_DUP_DUPLICATA");
        }
    }
}
