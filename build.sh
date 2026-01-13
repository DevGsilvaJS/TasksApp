#!/bin/bash

echo "🔨 Building TasksApp..."

# Build Frontend
echo "📦 Building Angular frontend..."
cd ui-taskapp
npm ci
npm run build -- --configuration production
cd ..

# Build Backend
echo "📦 Building .NET backend..."
dotnet restore
dotnet build -c Release

echo "✅ Build completed!"
