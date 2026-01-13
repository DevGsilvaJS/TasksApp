# ============================================
# Stage 1: Build do Frontend Angular
# ============================================
FROM node:20-alpine AS frontend-build
WORKDIR /app/ui-taskapp

# Copiar arquivos de dependências primeiro (cache layer)
COPY ui-taskapp/package*.json ./

# Instalar dependências (Angular precisa de devDependencies para build)
RUN npm install

# Copiar resto do código fonte do frontend
COPY ui-taskapp/ ./

# Build de produção do Angular
RUN npm run build -- --configuration production

# ============================================
# Stage 2: Build do Backend .NET
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app

# Copiar arquivos de projeto para restaurar dependências (cache layer)
COPY *.sln ./
COPY TasksAppAPI/*.csproj ./TasksAppAPI/
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/

# Restaurar dependências
RUN dotnet restore

# Copiar todo o código fonte
COPY . ./

# Publish do backend em Release (otimizado para produção)
RUN dotnet publish TasksAppAPI/TasksAppAPI.csproj -c Release -o /app/publish --no-restore

# ============================================
# Stage 3: Runtime (Produção)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar backend publicado
COPY --from=backend-build /app/publish ./

# Copiar frontend build para wwwroot
COPY --from=frontend-build /app/ui-taskapp/dist/ui-taskapp/browser ./wwwroot

# Variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production

# Render injeta PORT automaticamente via variável de ambiente
# O Program.cs está configurado para usar PORT dinamicamente
# NÃO defina porta fixa aqui - o Render gerencia isso

# Entrypoint
ENTRYPOINT ["dotnet", "TasksAppAPI.dll"]
