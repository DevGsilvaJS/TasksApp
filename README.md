# TasksApp

Sistema de gerenciamento de tarefas, clientes, usuários e contas a pagar.

## 🚀 Tecnologias

- **Backend**: .NET 8.0, Entity Framework Core, PostgreSQL
- **Frontend**: Angular 17
- **Banco de Dados**: PostgreSQL

## 📋 Pré-requisitos

- .NET 8.0 SDK
- Node.js 20+ e npm
- PostgreSQL 14+

## 🛠️ Desenvolvimento Local

### 1. Configurar Banco de Dados

Crie um banco de dados PostgreSQL e atualize a connection string em `TasksAppAPI/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TasksAppDB;Username=postgres;Password=sua_senha"
  }
}
```

### 2. Executar Migrations

```bash
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project TasksAppAPI/TasksAppAPI.csproj
```

### 3. Executar Backend

```bash
cd TasksAppAPI
dotnet run
```

O backend estará disponível em `http://localhost:5132`

### 4. Executar Frontend

```bash
cd ui-taskapp
npm install
npm start
```

O frontend estará disponível em `http://localhost:4200`

## 🐳 Docker (Desenvolvimento)

```bash
docker build -t tasksapp .
docker run -p 5000:5000 -e DATABASE_URL="sua_connection_string" tasksapp
```

## 🌐 Deploy no Render

### 1. Preparação

1. Crie uma conta no [Render](https://render.com)
2. Crie um banco de dados PostgreSQL no Render
3. Faça push do código para um repositório Git (GitHub, GitLab, etc.)

### 2. Configurar no Render

1. No dashboard do Render, clique em "New +" → "Web Service"
2. Conecte seu repositório Git
3. Configure:
   - **Name**: tasksapp-api
   - **Environment**: Docker
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.`
   - **Region**: Escolha a região mais próxima

### 3. Variáveis de Ambiente

Configure as seguintes variáveis de ambiente no Render:

- `ASPNETCORE_ENVIRONMENT`: `Production`
- `DATABASE_URL`: A connection string do seu banco PostgreSQL no Render (gerada automaticamente)
- `CorsOrigins`: URLs permitidas para CORS (ex: `https://seu-app.onrender.com;https://www.seudominio.com`)

### 4. Deploy Automático

O arquivo `render.yaml` está configurado para deploy automático. Basta fazer push para a branch principal.

### 5. Health Check

O Render verificará a saúde da aplicação através do endpoint `/swagger`.

## 📁 Estrutura do Projeto

```
TasksApp/
├── Application/          # Camada de aplicação (DTOs, Services, Interfaces)
├── Domain/              # Entidades de domínio
├── Infrastructure/      # Implementações (DbContext, Repositories, Migrations)
├── TasksAppAPI/        # API REST (Controllers, Program.cs)
├── ui-taskapp/         # Frontend Angular
├── Dockerfile          # Configuração Docker
├── render.yaml         # Configuração Render
└── README.md           # Este arquivo
```

## 🔐 Autenticação

O sistema possui autenticação por login e senha. Os dados do usuário são armazenados no `sessionStorage`.

## 📝 Funcionalidades

- ✅ Gerenciamento de Clientes
- ✅ Gerenciamento de Usuários
- ✅ Gerenciamento de Tarefas/Atendimentos
- ✅ Sistema de Anotações
- ✅ Upload de Imagens
- ✅ Contas a Pagar
- ✅ Sistema de Parcelas
- ✅ Autenticação e Autorização

## 🐛 Troubleshooting

### Erro de conexão com banco de dados

Verifique se:
- O PostgreSQL está rodando
- A connection string está correta
- As migrations foram aplicadas

### Erro de CORS

Em produção, configure a variável `CorsOrigins` com as URLs permitidas separadas por `;`.

### Build do Docker falha

Certifique-se de que:
- O Docker está instalado e rodando
- Todos os arquivos necessários estão no contexto do Docker
- As dependências estão corretas

## 📄 Licença

Este projeto é privado.
