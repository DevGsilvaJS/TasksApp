using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPlanoContasDuplicata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PLCID",
                table: "TB_DUP_DUPLICATA",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_DUP_DUPLICATA_PLCID",
                table: "TB_DUP_DUPLICATA",
                column: "PLCID");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_PLC_PLANO_CONTAS_PLCID",
                table: "TB_DUP_DUPLICATA",
                column: "PLCID",
                principalTable: "TB_PLC_PLANO_CONTAS",
                principalColumn: "PLCID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_PLC_PLANO_CONTAS_PLCID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropIndex(
                name: "IX_TB_DUP_DUPLICATA_PLCID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropColumn(
                name: "PLCID",
                table: "TB_DUP_DUPLICATA");
        }
    }
}
