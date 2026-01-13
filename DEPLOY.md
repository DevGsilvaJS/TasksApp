# 🚀 Guia de Deploy no Render

Este guia detalha como fazer o deploy do TasksApp no Render.

## 📋 Pré-requisitos

1. Conta no [Render](https://render.com)
2. Repositório Git (GitHub, GitLab, Bitbucket)
3. Código do projeto commitado e enviado para o repositório

## 🔧 Passo a Passo

### 1. Criar Banco de Dados PostgreSQL

1. No dashboard do Render, clique em **"New +"** → **"PostgreSQL"**
2. Configure:
   - **Name**: `tasksapp-db`
   - **Database**: `tasksappdb`
   - **User**: (será gerado automaticamente)
   - **Region**: Escolha a mesma região do seu web service
   - **Plan**: Escolha conforme sua necessidade (Starter é suficiente para começar)
3. Clique em **"Create Database"**
4. **IMPORTANTE**: Anote a **Internal Database URL** (será usada como `DATABASE_URL`)

### 2. Criar Web Service

1. No dashboard do Render, clique em **"New +"** → **"Web Service"**
2. Conecte seu repositório Git
3. Configure o serviço:
   - **Name**: `tasksapp-api`
   - **Environment**: `Docker`
   - **Region**: Escolha a mesma região do banco de dados
   - **Branch**: `main` (ou sua branch principal)
   - **Root Directory**: Deixe vazio (raiz do projeto)
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.`
   - **Plan**: Escolha conforme sua necessidade

### 3. Configurar Variáveis de Ambiente

Na seção **"Environment"** do web service, adicione:

| Key | Value | Descrição |
|-----|-------|-----------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente de execução |
| `DATABASE_URL` | `postgres://user:pass@host:port/dbname` | URL do banco (use a Internal Database URL do passo 1) |
| `CorsOrigins` | `https://tasksapp-api.onrender.com` | URLs permitidas para CORS (separadas por `;`) |

**Nota**: O Render pode gerar automaticamente a `DATABASE_URL` se você conectar o banco ao serviço.

### 4. Configurar Health Check (Opcional)

- **Health Check Path**: `/swagger`
- Isso permite que o Render verifique se a aplicação está funcionando

### 5. Deploy

1. Clique em **"Create Web Service"**
2. O Render começará a fazer o build automaticamente
3. Aguarde o build completar (pode levar 5-10 minutos na primeira vez)
4. Após o build, a aplicação estará disponível em `https://tasksapp-api.onrender.com`

### 6. Executar Migrations

Após o primeiro deploy, você precisa executar as migrations:

1. Acesse o **Shell** do seu web service no Render
2. Execute:
```bash
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project TasksAppAPI/TasksAppAPI.csproj
```

Ou configure um script de build que execute as migrations automaticamente.

## 🔄 Deploy Automático

O arquivo `render.yaml` está configurado para:
- Deploy automático ao fazer push para a branch principal
- Usar Docker para build
- Configurar variáveis de ambiente

## 🐛 Troubleshooting

### Build falha

- Verifique os logs de build no Render
- Certifique-se de que todas as dependências estão corretas
- Verifique se o Dockerfile está correto

### Erro de conexão com banco

- Verifique se a `DATABASE_URL` está correta
- Certifique-se de que o banco está na mesma região
- Verifique se as migrations foram executadas

### CORS errors

- Configure a variável `CorsOrigins` com a URL completa do seu serviço
- Exemplo: `https://tasksapp-api.onrender.com`

### Aplicação não inicia

- Verifique os logs de runtime
- Certifique-se de que a porta está configurada corretamente (Render usa porta dinâmica)
- Verifique se todas as variáveis de ambiente estão configuradas

## 📝 Notas Importantes

1. **Primeiro Deploy**: Pode levar mais tempo devido ao download de dependências
2. **Sleep Mode**: No plano gratuito, o serviço pode entrar em sleep após inatividade
3. **SSL**: O Render fornece SSL automático para todos os serviços
4. **Logs**: Acesse os logs em tempo real no dashboard do Render

## 🔗 Links Úteis

- [Documentação Render](https://render.com/docs)
- [Render Dashboard](https://dashboard.render.com)
