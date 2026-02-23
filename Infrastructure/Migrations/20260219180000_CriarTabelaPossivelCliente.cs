using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaPossivelCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_POSSIVEL_CLIENTE",
                columns: table => new
                {
                    POCID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    POCCODIGO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    POCLOJA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    POCSTATUS = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POCFANTASIA = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    POCDDD = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    POCCNPJ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    POCRAZAOSOCIAL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    POCEMAILCOMERCIAL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    POCCELDDD = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    POCCELULAR = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    POCDATAIMPORTACAO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_POSSIVEL_CLIENTE", x => x.POCID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_POSSIVEL_CLIENTE_POCCODIGO",
                table: "TB_POSSIVEL_CLIENTE",
                column: "POCCODIGO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TB_POSSIVEL_CLIENTE");
        }
    }
}
