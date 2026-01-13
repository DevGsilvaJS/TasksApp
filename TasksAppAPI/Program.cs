using Infrastructure.Extensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Services
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS simples (Angular + API juntos)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ======================
// Database
// ======================

// Lê a variável de ambiente DATABASE_URL (Render) ou do appsettings
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL não encontrada.");

// Converter URL do Render (postgres://) para formato Npgsql
// Apenas se for postgres://
if (databaseUrl.StartsWith("postgres://"))
{
    try
    {
        var uri = new Uri(databaseUrl);
        
        // Extrair username e password do UserInfo
        // O UserInfo já vem decodificado pelo Uri, mas pode ter caracteres especiais
        var userInfo = uri.UserInfo;
        var colonIndex = userInfo.IndexOf(':');
        
        if (colonIndex < 0)
        {
            throw new InvalidOperationException($"Formato inválido de DATABASE_URL. UserInfo deve conter 'user:password', recebido: '{userInfo}'");
        }
        
        var username = userInfo.Substring(0, colonIndex);
        var password = userInfo.Substring(colonIndex + 1);
        
        // Decode adicional se necessário (para caracteres URL-encoded)
        username = Uri.UnescapeDataString(username);
        password = Uri.UnescapeDataString(password);

        var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.LocalPath.TrimStart('/'),
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };

        databaseUrl = npgsqlBuilder.ConnectionString;
        
        Console.WriteLine($"✅ Connection string convertida. Host: {npgsqlBuilder.Host}, Port: {npgsqlBuilder.Port}, Database: {npgsqlBuilder.Database}, User: {npgsqlBuilder.Username}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao converter DATABASE_URL: {ex.Message}");
        Console.WriteLine($"   DATABASE_URL recebida: {(string.IsNullOrEmpty(databaseUrl) ? "(vazia)" : databaseUrl.Substring(0, Math.Min(50, databaseUrl.Length)) + "...")}");
        throw new InvalidOperationException($"Erro ao converter DATABASE_URL: {ex.Message}", ex);
    }
}

// Validar connection string antes de usar
if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new InvalidOperationException("Connection string está vazia após conversão.");
}

// Injetar a connection string convertida no IConfiguration
// Isso permite que AddInfrastructure leia corretamente
builder.Configuration["ConnectionStrings:DefaultConnection"] = databaseUrl;

// Adiciona DbContext e serviços usando AddInfrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// ======================
// Static files (Angular)
// ======================
builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "wwwroot";
});

var app = builder.Build();

// Porta dinâmica do Render (produção)
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Clear();
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// ======================
// Middleware / Pipeline
// ======================
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Fallback para Angular Router
app.MapFallbackToFile("index.html");

// ======================
// Migrations automáticas
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    Console.WriteLine("✅ Migrations aplicadas com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao aplicar migrations: {ex.Message}");
    // Não interrompe a aplicação se migrations falharem
}

app.Run();
