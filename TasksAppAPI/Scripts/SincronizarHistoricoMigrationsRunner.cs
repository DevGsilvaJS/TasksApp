using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TasksAppAPI.Scripts;

/// <summary>
/// Registra no histórico do EF migrations cujos objetos já existem no banco
/// (criados por scripts de startup anteriores), evitando erro 42P07 no Migrate().
/// </summary>
public static class SincronizarHistoricoMigrationsRunner
{
    public static void Executar(ApplicationDbContext db)
    {
        const string sql = """
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260219180000_CriarTabelaPossivelCliente', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'TB_POSSIVEL_CLIENTE'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260219180000_CriarTabelaPossivelCliente'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260527142624_CriarTabelaClienteContratoValor', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'TB_CAD_STATUS_ATEND_COMERCIAL'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260527142624_CriarTabelaClienteContratoValor'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260612133249_CriarTabelasEmpresaCentroCustoPlanoContas', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'TB_EMP_EMPRESA'
        )
        AND EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'TB_CCU_CENTRO_CUSTO'
        )
        AND EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'TB_PLC_PLANO_CONTAS'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260612133249_CriarTabelasEmpresaCentroCustoPlanoContas'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260612145500_AdicionarCentroCustoPlanoContasParcela', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_PAR_PARCELA'
              AND column_name = 'CCUID'
        )
        AND EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_PAR_PARCELA'
              AND column_name = 'PLCID'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260612145500_AdicionarCentroCustoPlanoContasParcela'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260612141051_AdicionarCentroCustoDuplicata', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_DUP_DUPLICATA'
              AND column_name = 'CCUID'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260612141051_AdicionarCentroCustoDuplicata'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260612141735_AdicionarPlanoContasDuplicata', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_DUP_DUPLICATA'
              AND column_name = 'PLCID'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260612141735_AdicionarPlanoContasDuplicata'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260617142053_CriarTabelaEmailEnvioComercial', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name = 'TB_EMAIL_ENVIO_COMERCIAL'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260617142053_CriarTabelaEmailEnvioComercial'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260619120500_AdicionarCongeladaPorClienteParcela', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_PAR_PARCELA'
              AND column_name = 'PARCONGELADAPORCLIENTE'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260619120500_AdicionarCongeladaPorClienteParcela'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260619123000_AdicionarInativaDuplicata', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_DUP_DUPLICATA'
              AND column_name = 'DUPINATIVA'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260619123000_AdicionarInativaDuplicata'
        );

        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20260703210000_AdicionarTipoObservacoesAnotacao', '8.0.0'
        WHERE EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_ANO_ANOTACAO'
              AND column_name = 'ANOTIPO'
        )
        AND EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'TB_ANO_ANOTACAO'
              AND column_name = 'ANOOBSERVACOES'
        )
        AND NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260703210000_AdicionarTipoObservacoesAnotacao'
        );
        """;

        var inseridos = db.Database.ExecuteSqlRaw(sql);
        if (inseridos > 0)
            Console.WriteLine($"✅ Histórico de migrations sincronizado ({inseridos} registro(s) adicionado(s)).");
    }
}
