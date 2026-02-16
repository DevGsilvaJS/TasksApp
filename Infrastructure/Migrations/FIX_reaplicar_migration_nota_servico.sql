-- Execute este script no PostgreSQL se a migration foi aplicada vazia e a coluna CLIDIANFSERVICO não existe.
-- Isso remove o registro da migration para que, ao reiniciar a API, ela seja aplicada novamente (com o Up preenchido).

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260216175647_TrocarDataEnvioPorDiaNfServicoECriarEnvioNotaServico';
