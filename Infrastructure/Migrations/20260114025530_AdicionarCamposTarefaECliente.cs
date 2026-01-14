using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposTarefaECliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TARPRIORIDADE",
                table: "TB_TAR_TAREFAS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TARTIPOATENDIMENTO",
                table: "TB_TAR_TAREFAS",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CLIDATAFINALCONTRATO",
                table: "TB_CLI_CLIENTE",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CLIDATAPAGAMENTO",
                table: "TB_CLI_CLIENTE",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CLISTATUS",
                table: "TB_CLI_CLIENTE",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TARPRIORIDADE",
                table: "TB_TAR_TAREFAS");

            migrationBuilder.DropColumn(
                name: "TARTIPOATENDIMENTO",
                table: "TB_TAR_TAREFAS");

            migrationBuilder.DropColumn(
                name: "CLIDATAFINALCONTRATO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.DropColumn(
                name: "CLIDATAPAGAMENTO",
                table: "TB_CLI_CLIENTE");

            migrationBuilder.DropColumn(
                name: "CLISTATUS",
                table: "TB_CLI_CLIENTE");
        }
    }
}
