using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTituloRegimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "REGTITULO",
                table: "TB_REG_REGIMENTO",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "TB_REG_REGIMENTO"
                SET "REGTITULO" = LEFT("REGDESCRICAO", 300)
                WHERE "REGTITULO" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "REGTITULO",
                table: "TB_REG_REGIMENTO");
        }
    }
}
