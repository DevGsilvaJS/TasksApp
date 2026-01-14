using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNumeroAtendimentoEAlterarDescricao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TARNUMERO",
                table: "TB_TAR_TAREFAS",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ANTDESCRICAO",
                table: "TB_ANT_ANOTACOES_TAREFAS",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TARNUMERO",
                table: "TB_TAR_TAREFAS");

            migrationBuilder.AlterColumn<string>(
                name: "ANTDESCRICAO",
                table: "TB_ANT_ANOTACOES_TAREFAS",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
