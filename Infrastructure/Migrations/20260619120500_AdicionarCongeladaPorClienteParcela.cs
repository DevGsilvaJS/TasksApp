using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260619120500_AdicionarCongeladaPorClienteParcela")]
    /// <inheritdoc />
    public partial class AdicionarCongeladaPorClienteParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PARCONGELADAPORCLIENTE",
                table: "TB_PAR_PARCELA",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PARCONGELADAPORCLIENTE",
                table: "TB_PAR_PARCELA");
        }
    }
}
