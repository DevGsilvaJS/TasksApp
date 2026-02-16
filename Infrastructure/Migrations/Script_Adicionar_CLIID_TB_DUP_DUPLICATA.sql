-- Script para adicionar a coluna CLIID na tabela TB_DUP_DUPLICATA.
-- Execute no banco que está dando "coluna t.CLIID não existe".
-- Pode rodar no pgAdmin, DBeaver, psql, etc.

-- 1) Adicionar coluna (idempotente no PostgreSQL 9.5+)
ALTER TABLE "TB_DUP_DUPLICATA" ADD COLUMN IF NOT EXISTS "CLIID" integer NULL;

-- 2) Índice
CREATE INDEX IF NOT EXISTS "IX_TB_DUP_DUPLICATA_CLIID" ON "TB_DUP_DUPLICATA" ("CLIID");

-- 3) FK (só se ainda não existir)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'FK_TB_DUP_DUPLICATA_TB_CLI_CLIENTE_CLIID'
    ) THEN
        ALTER TABLE "TB_DUP_DUPLICATA"
        ADD CONSTRAINT "FK_TB_DUP_DUPLICATA_TB_CLI_CLIENTE_CLIID"
        FOREIGN KEY ("CLIID") REFERENCES "TB_CLI_CLIENTE" ("CLIID") ON DELETE SET NULL;
    END IF;
END $$;
