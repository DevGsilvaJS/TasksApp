-- Define todos os usuários existentes como Administrador (perfil 1).
-- Use após a migração de perfil para quem já tinha conta e ficou com Comercial (2).
-- Para afetar só um usuário: adicione por exemplo AND "USULOGIN" = 'seu_login'

UPDATE "TB_USU_USUARIO"
SET "USUPERFIL" = 1
WHERE "USUPERFIL" = 2;
