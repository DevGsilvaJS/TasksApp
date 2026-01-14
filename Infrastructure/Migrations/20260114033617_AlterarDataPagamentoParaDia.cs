using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterarDataPagamentoParaDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLIDATAPAGAMENTO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.AddColumn<int>(
                name: "CLIDIAPAGAMENTO",
                table: "TB_CLI_CLIENTE",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLIDIAPAGAMENTO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.AddColumn<DateTime>(
                name: "CLIDATAPAGAMENTO",
                table: "TB_CLI_CLIENTE",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
