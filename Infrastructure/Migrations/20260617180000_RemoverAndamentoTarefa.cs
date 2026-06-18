using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class RemoverAndamentoTarefa : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TARANDAMENTO",
            table: "TB_TAR_TAREFAS");

        migrationBuilder.DropTable(
            name: "TB_CAD_ANDAMENTO");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TB_CAD_ANDAMENTO",
            columns: table => new
            {
                ANID = table.Column<int>(type: "integer", nullable: false),
                ANDESCRICAO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ANATIVO = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_CAD_ANDAMENTO", x => x.ANID);
            });

        migrationBuilder.AddColumn<int>(
            name: "TARANDAMENTO",
            table: "TB_TAR_TAREFAS",
            type: "integer",
            nullable: false,
            defaultValue: 1);
    }
}
