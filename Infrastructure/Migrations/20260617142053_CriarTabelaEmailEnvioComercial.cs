using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaEmailEnvioComercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_EMAIL_ENVIO_COMERCIAL",
                columns: table => new
                {
                    EECID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EECEMAIL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EECNAOENVIAR = table.Column<bool>(type: "boolean", nullable: false),
                    EECDATACADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_EMAIL_ENVIO_COMERCIAL", x => x.EECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_EMAIL_ENVIO_COMERCIAL_EECEMAIL",
                table: "TB_EMAIL_ENVIO_COMERCIAL",
                column: "EECEMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_EMAIL_ENVIO_COMERCIAL");
        }
    }
}
