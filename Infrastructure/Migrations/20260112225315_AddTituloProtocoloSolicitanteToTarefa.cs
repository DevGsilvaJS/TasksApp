using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTituloProtocoloSolicitanteToTarefa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TARPROTOCOLO",
                table: "TB_TAR_TAREFAS",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TARSOLICITANTE",
                table: "TB_TAR_TAREFAS",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TARTITULO",
                table: "TB_TAR_TAREFAS",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TARPROTOCOLO",
                table: "TB_TAR_TAREFAS");

            migrationBuilder.DropColumn(
                name: "TARSOLICITANTE",
                table: "TB_TAR_TAREFAS");

            migrationBuilder.DropColumn(
                name: "TARTITULO",
                table: "TB_TAR_TAREFAS");
        }
    }
}
