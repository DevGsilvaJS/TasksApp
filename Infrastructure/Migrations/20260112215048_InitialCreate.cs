using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_PESSOA",
                columns: table => new
                {
                    PESID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PESNOME = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PESSOBRENOME = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PESDOCFEDERAL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PESDOCESTADUAL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PESSOA", x => x.PESID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLI_CLIENTE",
                columns: table => new
                {
                    CLIID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PESID = table.Column<int>(type: "integer", nullable: false),
                    CLICODIGO = table.Column<int>(type: "integer", nullable: false),
                    CLIDATACADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLI_CLIENTE", x => x.CLIID);
                    table.ForeignKey(
                        name: "FK_TB_CLI_CLIENTE_TB_PESSOA_PESID",
                        column: x => x.PESID,
                        principalTable: "TB_PESSOA",
                        principalColumn: "PESID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_EMAIL",
                columns: table => new
                {
                    EMLID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PESID = table.Column<int>(type: "integer", nullable: false),
                    EMLDESCRICAO = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_EMAIL", x => x.EMLID);
                    table.ForeignKey(
                        name: "FK_TB_EMAIL_TB_PESSOA_PESID",
                        column: x => x.PESID,
                        principalTable: "TB_PESSOA",
                        principalColumn: "PESID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_USU_USUARIO",
                columns: table => new
                {
                    USUID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PESID = table.Column<int>(type: "integer", nullable: false),
                    USULOGIN = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    USUSENHA = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USU_USUARIO", x => x.USUID);
                    table.ForeignKey(
                        name: "FK_TB_USU_USUARIO_TB_PESSOA_PESID",
                        column: x => x.PESID,
                        principalTable: "TB_PESSOA",
                        principalColumn: "PESID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_TAR_TAREFAS",
                columns: table => new
                {
                    TARID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLIID = table.Column<int>(type: "integer", nullable: false),
                    USUID = table.Column<int>(type: "integer", nullable: false),
                    TARDTCADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TARDTCONCLUSAO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TARSTATUS = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TAR_TAREFAS", x => x.TARID);
                    table.ForeignKey(
                        name: "FK_TB_TAR_TAREFAS_TB_CLI_CLIENTE_CLIID",
                        column: x => x.CLIID,
                        principalTable: "TB_CLI_CLIENTE",
                        principalColumn: "CLIID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_TAR_TAREFAS_TB_USU_USUARIO_USUID",
                        column: x => x.USUID,
                        principalTable: "TB_USU_USUARIO",
                        principalColumn: "USUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_ANT_ANOTACOES_TAREFAS",
                columns: table => new
                {
                    ANTID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TARID = table.Column<int>(type: "integer", nullable: false),
                    USUID = table.Column<int>(type: "integer", nullable: false),
                    ANTDESCRICAO = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ANTDTCADASTRO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ANT_ANOTACOES_TAREFAS", x => x.ANTID);
                    table.ForeignKey(
                        name: "FK_TB_ANT_ANOTACOES_TAREFAS_TB_TAR_TAREFAS_TARID",
                        column: x => x.TARID,
                        principalTable: "TB_TAR_TAREFAS",
                        principalColumn: "TARID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_ANT_ANOTACOES_TAREFAS_TB_USU_USUARIO_USUID",
                        column: x => x.USUID,
                        principalTable: "TB_USU_USUARIO",
                        principalColumn: "USUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_IMT_IMAGEM_TAREFA",
                columns: table => new
                {
                    IMGID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TARID = table.Column<int>(type: "integer", nullable: false),
                    IMGARQUIVO = table.Column<byte[]>(type: "bytea", nullable: true),
                    IMGDATAARQUIVO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_IMT_IMAGEM_TAREFA", x => x.IMGID);
                    table.ForeignKey(
                        name: "FK_TB_IMT_IMAGEM_TAREFA_TB_TAR_TAREFAS_TARID",
                        column: x => x.TARID,
                        principalTable: "TB_TAR_TAREFAS",
                        principalColumn: "TARID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ANT_ANOTACOES_TAREFAS_TARID",
                table: "TB_ANT_ANOTACOES_TAREFAS",
                column: "TARID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ANT_ANOTACOES_TAREFAS_USUID",
                table: "TB_ANT_ANOTACOES_TAREFAS",
                column: "USUID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLI_CLIENTE_PESID",
                table: "TB_CLI_CLIENTE",
                column: "PESID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_EMAIL_PESID",
                table: "TB_EMAIL",
                column: "PESID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_IMT_IMAGEM_TAREFA_TARID",
                table: "TB_IMT_IMAGEM_TAREFA",
                column: "TARID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_TAR_TAREFAS_CLIID",
                table: "TB_TAR_TAREFAS",
                column: "CLIID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_TAR_TAREFAS_USUID",
                table: "TB_TAR_TAREFAS",
                column: "USUID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_USU_USUARIO_PESID",
                table: "TB_USU_USUARIO",
                column: "PESID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_ANT_ANOTACOES_TAREFAS");

            migrationBuilder.DropTable(
                name: "TB_EMAIL");

            migrationBuilder.DropTable(
                name: "TB_IMT_IMAGEM_TAREFA");

            migrationBuilder.DropTable(
                name: "TB_TAR_TAREFAS");

            migrationBuilder.DropTable(
                name: "TB_CLI_CLIENTE");

            migrationBuilder.DropTable(
                name: "TB_USU_USUARIO");

            migrationBuilder.DropTable(
                name: "TB_PESSOA");
        }
    }
}
