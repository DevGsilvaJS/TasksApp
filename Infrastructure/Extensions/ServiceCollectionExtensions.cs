using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar PostgreSQL para usar UTC em DateTime
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
        
        // Configurar PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' está vazia ou não foi encontrada.");
        }
        
        // Validar se a connection string está no formato correto do Npgsql
        try
        {
            var testBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            System.Console.WriteLine($"✅ Connection string válida. Host: {testBuilder.Host}, Database: {testBuilder.Database}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Connection string inválida: {ex.Message}");
            System.Console.WriteLine($"   Connection string recebida: {(connectionString.Length > 100 ? connectionString.Substring(0, 100) + "..." : connectionString)}");
            throw;
        }
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registrar repositório genérico
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Registrar services
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITarefaService, TarefaService>();
        services.AddScoped<IAnotacaoService, AnotacaoService>();
        services.AddScoped<IDuplicataService, DuplicataService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
