# Script de build para Windows PowerShell

Write-Host "🔨 Building TasksApp..." -ForegroundColor Cyan

# Build Frontend
Write-Host "📦 Building Angular frontend..." -ForegroundColor Yellow
Set-Location ui-taskapp
npm ci
npm run build -- --configuration production
Set-Location ..

# Build Backend
Write-Host "📦 Building .NET backend..." -ForegroundColor Yellow
dotnet restore
dotnet build -c Release

Write-Host "✅ Build completed!" -ForegroundColor Green
