using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenomearNomeParaFantasiaRemoverSobrenome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renomear coluna PESNOME para PESFANTASIA
            migrationBuilder.RenameColumn(
                name: "PESNOME",
                table: "TB_PESSOA",
                newName: "PESFANTASIA");

            // Remover coluna PESSOBRENOME
            migrationBuilder.DropColumn(
                name: "PESSOBRENOME",
                table: "TB_PESSOA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna PESSOBRENOME de volta
            migrationBuilder.AddColumn<string>(
                name: "PESSOBRENOME",
                table: "TB_PESSOA",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            // Renomear coluna PESFANTASIA de volta para PESNOME
            migrationBuilder.RenameColumn(
                name: "PESFANTASIA",
                table: "TB_PESSOA",
                newName: "PESNOME");
        }
    }
}
