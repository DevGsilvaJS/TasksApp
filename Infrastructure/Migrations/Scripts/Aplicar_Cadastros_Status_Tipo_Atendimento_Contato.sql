-- Migração: CriarCadastrosStatusTipoAtendimentoTipoContato
-- Execute este script no mesmo banco que a API usa se as tabelas ainda não existirem.

CREATE TABLE IF NOT EXISTS "TB_CAD_STATUS_TAREFA" (
    "STCID" integer NOT NULL,
    "STCDESCRICAO" character varying(100) NOT NULL,
    "STCATIVO" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_TB_CAD_STATUS_TAREFA" PRIMARY KEY ("STCID")
);

CREATE TABLE IF NOT EXISTS "TB_CAD_TIPO_ATENDIMENTO" (
    "TAID" integer NOT NULL,
    "TADESCRICAO" character varying(100) NOT NULL,
    "TAATIVO" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_TB_CAD_TIPO_ATENDIMENTO" PRIMARY KEY ("TAID")
);

CREATE TABLE IF NOT EXISTS "TB_CAD_TIPO_CONTATO" (
    "TCID" integer NOT NULL,
    "TCDESCRICAO" character varying(100) NOT NULL,
    "TCATIVO" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_TB_CAD_TIPO_CONTATO" PRIMARY KEY ("TCID")
);

INSERT INTO "TB_CAD_STATUS_TAREFA" ("STCID", "STCDESCRICAO", "STCATIVO") VALUES
(1, 'Em Aberto', true),
(2, 'Concluída', true),
(3, 'Cancelada', true),
(4, 'Reativada', true),
(5, 'Aguardando Cliente', true)
ON CONFLICT ("STCID") DO NOTHING;

INSERT INTO "TB_CAD_TIPO_ATENDIMENTO" ("TAID", "TADESCRICAO", "TAATIVO") VALUES
(1, 'Treinamento', true),
(2, 'Suporte', true),
(3, 'Reunião', true),
(4, 'Cobrança', true)
ON CONFLICT ("TAID") DO NOTHING;

INSERT INTO "TB_CAD_TIPO_CONTATO" ("TCID", "TCDESCRICAO", "TCATIVO") VALUES
(1, 'Ligação', true),
(2, 'WhatsApp', true),
(3, 'E-mail', true)
ON CONFLICT ("TCID") DO NOTHING;

-- Registrar a migração no histórico do EF (para que dotnet ef não tente aplicá-la de novo)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260220120000_CriarCadastrosStatusTipoAtendimentoTipoContato', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
