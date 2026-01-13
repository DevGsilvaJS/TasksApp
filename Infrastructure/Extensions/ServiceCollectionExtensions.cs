using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar PostgreSQL para usar UTC em DateTime
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
        
        // Configurar PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
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

        return services;
    }
}
