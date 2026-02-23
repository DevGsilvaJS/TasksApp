using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarCadastrosStatusTipoAtendimentoTipoContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_CAD_STATUS_TAREFA",
                columns: table => new
                {
                    STCID = table.Column<int>(type: "integer", nullable: false),
                    STCDESCRICAO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    STCATIVO = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_STATUS_TAREFA", x => x.STCID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CAD_TIPO_ATENDIMENTO",
                columns: table => new
                {
                    TAID = table.Column<int>(type: "integer", nullable: false),
                    TADESCRICAO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TAATIVO = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_TIPO_ATENDIMENTO", x => x.TAID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CAD_TIPO_CONTATO",
                columns: table => new
                {
                    TCID = table.Column<int>(type: "integer", nullable: false),
                    TCDESCRICAO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TCATIVO = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_TIPO_CONTATO", x => x.TCID);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""TB_CAD_STATUS_TAREFA"" (""STCID"", ""STCDESCRICAO"", ""STCATIVO"") VALUES
                (1, 'Em Aberto', true),
                (2, 'Concluída', true),
                (3, 'Cancelada', true),
                (4, 'Reativada', true),
                (5, 'Aguardando Cliente', true)
                ON CONFLICT (""STCID"") DO NOTHING;
            ");
            migrationBuilder.Sql(@"
                INSERT INTO ""TB_CAD_TIPO_ATENDIMENTO"" (""TAID"", ""TADESCRICAO"", ""TAATIVO"") VALUES
                (1, 'Treinamento', true),
                (2, 'Suporte', true),
                (3, 'Reunião', true),
                (4, 'Cobrança', true)
                ON CONFLICT (""TAID"") DO NOTHING;
            ");
            migrationBuilder.Sql(@"
                INSERT INTO ""TB_CAD_TIPO_CONTATO"" (""TCID"", ""TCDESCRICAO"", ""TCATIVO"") VALUES
                (1, 'Ligação', true),
                (2, 'WhatsApp', true),
                (3, 'E-mail', true)
                ON CONFLICT (""TCID"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TB_CAD_STATUS_TAREFA");
            migrationBuilder.DropTable(name: "TB_CAD_TIPO_ATENDIMENTO");
            migrationBuilder.DropTable(name: "TB_CAD_TIPO_CONTATO");
        }
    }
}
