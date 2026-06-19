# ============================================
# Stage 1: Build do Frontend Angular
# ============================================
FROM node:20-alpine AS frontend-build
WORKDIR /app/ui-taskapp

ARG RENDER_GIT_COMMIT=unknown

# Copiar arquivos de dependências primeiro (cache layer)
COPY ui-taskapp/package*.json ./

# Instalar dependências (Angular precisa de devDependencies para build)
RUN npm install

# Copiar resto do código fonte do frontend
COPY ui-taskapp/ ./

# Build de produção do Angular + validação das funcionalidades exigidas
RUN npm run build -- --configuration production \
    && grep -rq "inativar-parcelas-restantes" dist/ui-taskapp/browser \
    && grep -rq "contas_receber_agrupar_por" dist/ui-taskapp/browser \
    && grep -rq "dataPagamento" dist/ui-taskapp/browser \
    && echo "Frontend build verification passed (${RENDER_GIT_COMMIT})."

# ============================================
# Stage 2: Build do Backend .NET
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app

ARG RENDER_GIT_COMMIT=unknown

# Copiar arquivos de projeto para restaurar dependências (cache layer)
COPY *.sln ./
COPY TasksAppAPI/*.csproj ./TasksAppAPI/
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/

# Restaurar dependências
RUN dotnet restore

# Copiar todo o código fonte (wwwroot antigo é excluído via .dockerignore)
COPY . ./

# Validação do backend antes do publish
RUN grep -q "InativarParcelasRestantesAsync" Application/Services/DuplicataService.cs \
    && grep -q "DataPagamento" Application/DTOs/BaixarParcelaDto.cs \
    && echo "Backend source verification passed (${RENDER_GIT_COMMIT})."

# Publish do backend em Release (otimizado para produção)
RUN dotnet publish TasksAppAPI/TasksAppAPI.csproj -c Release -o /app/publish --no-restore

# ============================================
# Stage 3: Runtime (Produção)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ARG RENDER_GIT_COMMIT=unknown
ENV APP_VERSION=${RENDER_GIT_COMMIT}

# Copiar backend publicado
COPY --from=backend-build /app/publish ./

# Copiar frontend build para wwwroot (única fonte do Angular em produção)
COPY --from=frontend-build /app/ui-taskapp/dist/ui-taskapp/browser ./wwwroot

# Metadados de versão consultáveis via GET /api/health/version
RUN printf '{"commit":"%s","builtAt":"%s","features":["agrupamento-coluna","baixa-retroativa","inativar-parcelas-restantes"]}\n' \
    "${RENDER_GIT_COMMIT}" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > ./wwwroot/app-version.json

# Variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production

# Render injeta PORT automaticamente via variável de ambiente
ENTRYPOINT ["dotnet", "TasksAppAPI.dll"]
