-- Corrige histórico do EF quando tabelas já existem (criadas por scripts de startup).
-- Execute no banco TasksAppDB se o log mostrar erro ao criar TB_CAD_STATUS_ATEND_COMERCIAL.

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
AND NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260612133249_CriarTabelasEmpresaCentroCustoPlanoContas'
);
