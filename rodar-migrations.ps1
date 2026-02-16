# Rodar migrations a partir da raiz do repo (TasksApp)
# Uso: .\rodar-migrations.ps1

Set-Location -Path $PSScriptRoot
dotnet ef database update --project "Infrastructure\Infrastructure.csproj" --startup-project "TasksAppAPI\TasksAppAPI.csproj"
