using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260610100000_CriarCadastroAndamento")]
    /// <inheritdoc />
    public partial class CriarCadastroAndamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql(@"
                INSERT INTO ""TB_CAD_ANDAMENTO"" (""ANID"", ""ANDESCRICAO"", ""ANATIVO"") VALUES
                (1, 'A FAZER', true),
                (2, 'EM ANDAMENTO', true),
                (3, 'TESTAR', true),
                (4, 'RESOLVIDO', true)
                ON CONFLICT (""ANID"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TB_CAD_ANDAMENTO");
        }
    }
}
