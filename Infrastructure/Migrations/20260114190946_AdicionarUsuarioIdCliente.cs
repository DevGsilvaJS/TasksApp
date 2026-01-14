using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsuarioIdCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna como nullable primeiro
            migrationBuilder.AddColumn<int>(
                name: "USUID",
                table: "TB_CLI_CLIENTE",
                type: "integer",
                nullable: true);

            // Atualizar clientes existentes com o primeiro usuário disponível
            migrationBuilder.Sql(@"
                UPDATE ""TB_CLI_CLIENTE""
                SET ""USUID"" = (
                    SELECT ""USUID"" 
                    FROM ""TB_USU_USUARIO"" 
                    ORDER BY ""USUID"" 
                    LIMIT 1
                )
                WHERE ""USUID"" IS NULL;
            ");

            // Tornar a coluna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "USUID",
                table: "TB_CLI_CLIENTE",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLI_CLIENTE_USUID",
                table: "TB_CLI_CLIENTE",
                column: "USUID");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_CLI_CLIENTE_TB_USU_USUARIO_USUID",
                table: "TB_CLI_CLIENTE",
                column: "USUID",
                principalTable: "TB_USU_USUARIO",
                principalColumn: "USUID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_CLI_CLIENTE_TB_USU_USUARIO_USUID",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.DropIndex(
                name: "IX_TB_CLI_CLIENTE_USUID",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.DropColumn(
                name: "USUID",
                table: "TB_CLI_CLIENTE");
        }
    }
}
