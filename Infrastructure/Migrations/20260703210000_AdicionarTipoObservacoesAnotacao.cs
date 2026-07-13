using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260703210000_AdicionarTipoObservacoesAnotacao")]
    public partial class AdicionarTipoObservacoesAnotacao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ANOTIPO",
                table: "TB_ANO_ANOTACAO",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ANOTACAO");

            migrationBuilder.AddColumn<string>(
                name: "ANOOBSERVACOES",
                table: "TB_ANO_ANOTACAO",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ANOOBSERVACOES",
                table: "TB_ANO_ANOTACAO");

            migrationBuilder.DropColumn(
                name: "ANOTIPO",
                table: "TB_ANO_ANOTACAO");
        }
    }
}
