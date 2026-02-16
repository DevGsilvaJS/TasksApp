using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarClienteIdDuplicata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CLIID",
                table: "TB_DUP_DUPLICATA",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_DUP_DUPLICATA_CLIID",
                table: "TB_DUP_DUPLICATA",
                column: "CLIID");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_CLI_CLIENTE_CLIID",
                table: "TB_DUP_DUPLICATA",
                column: "CLIID",
                principalTable: "TB_CLI_CLIENTE",
                principalColumn: "CLIID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_DUP_DUPLICATA_TB_CLI_CLIENTE_CLIID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropIndex(
                name: "IX_TB_DUP_DUPLICATA_CLIID",
                table: "TB_DUP_DUPLICATA");

            migrationBuilder.DropColumn(
                name: "CLIID",
                table: "TB_DUP_DUPLICATA");
        }
    }
}
