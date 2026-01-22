using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTipoDuplicata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DUPTIPO",
                table: "TB_DUP_DUPLICATA",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "CP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DUPTIPO",
                table: "TB_DUP_DUPLICATA");
        }
    }
}
