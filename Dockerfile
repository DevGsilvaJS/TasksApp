# Build do frontend Angular
FROM node:20-alpine AS frontend-build
WORKDIR /app/ui-taskapp
COPY ui-taskapp/package*.json ./
RUN npm ci
COPY ui-taskapp/ ./
RUN npm run build -- --configuration production

# Build do backend .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY *.sln ./
COPY TasksAppAPI/*.csproj ./TasksAppAPI/
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/
RUN dotnet restore
COPY . ./
RUN dotnet build -c Release -o /app/build

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar backend compilado
COPY --from=backend-build /app/build ./

# Copiar frontend build para wwwroot
COPY --from=frontend-build /app/ui-taskapp/dist/ui-taskapp/browser ./wwwroot

# Configurar para servir arquivos estáticos
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "TasksAppAPI.dll"]
