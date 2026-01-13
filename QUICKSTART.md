# 🚀 Quick Start - Deploy no Render

## Passos Rápidos

### 1. Preparar Repositório
```bash
git add .
git commit -m "Preparar para deploy"
git push origin main
```

### 2. No Render Dashboard

1. **Criar PostgreSQL Database**
   - New + → PostgreSQL
   - Name: `tasksapp-db`
   - Plan: Starter
   - Create Database
   - **Copiar a Internal Database URL**

2. **Criar Web Service**
   - New + → Web Service
   - Conectar repositório Git
   - Configurar:
     - Name: `tasksapp-api`
     - Environment: `Docker`
     - Dockerfile Path: `./Dockerfile`
     - Docker Context: `.`
     - Plan: Starter

3. **Variáveis de Ambiente**
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `DATABASE_URL` = (cole a Internal Database URL do passo 1)
   - `CorsOrigins` = `https://tasksapp-api.onrender.com` (ajuste com seu URL)

4. **Create Web Service**

### 3. Executar Migrations

Após o primeiro deploy, acesse o Shell do serviço e execute:

```bash
cd /app
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project TasksAppAPI/TasksAppAPI.csproj
```

**Nota**: Você precisará instalar o dotnet-ef primeiro:
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"
```

### 4. Acessar Aplicação

Acesse: `https://tasksapp-api.onrender.com`

## ✅ Checklist

- [ ] Repositório Git configurado
- [ ] PostgreSQL criado no Render
- [ ] Web Service criado
- [ ] Variáveis de ambiente configuradas
- [ ] Migrations executadas
- [ ] Aplicação acessível

## 🐛 Problemas Comuns

**Build falha**: Verifique os logs no Render
**Erro 500**: Execute as migrations
**CORS error**: Configure `CorsOrigins` corretamente
