using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

/// <summary>
/// Usado pelo EF Core em tempo de design (dotnet ef) para usar a mesma connection string da API.
/// Garante que as migrações sejam aplicadas no mesmo banco que a API usa.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveApiProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = ResolveConnectionString(config);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string não encontrada. Defina DefaultConnection em appsettings ou DATABASE_URL no ambiente.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var infrastructureDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var apiPath = Path.Combine(infrastructureDir, "TasksAppAPI");
        if (!Directory.Exists(apiPath))
            apiPath = Path.Combine(Directory.GetCurrentDirectory(), "TasksAppAPI");
        if (!Directory.Exists(apiPath))
            throw new InvalidOperationException($"Pasta TasksAppAPI não encontrada. Procurou em: {apiPath}");
        return apiPath;
    }

    private static string ResolveConnectionString(IConfiguration config)
    {
        var databaseUrl = config["DATABASE_URL"]
            ?? config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(databaseUrl))
            return string.Empty;

        if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        try
        {
            var url = databaseUrl.Replace("postgresql://", "postgres://", StringComparison.OrdinalIgnoreCase);
            var uri = new Uri(url);
            var userInfo = uri.UserInfo;
            var colonIndex = userInfo.IndexOf(':');
            if (colonIndex < 0)
                return databaseUrl;
            var username = Uri.UnescapeDataString(userInfo.Substring(0, colonIndex));
            var password = Uri.UnescapeDataString(userInfo.Substring(colonIndex + 1));
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.LocalPath.TrimStart('/'),
                Username = username,
                Password = password,
                SslMode = Npgsql.SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }
        catch
        {
            return databaseUrl;
        }
    }
}
