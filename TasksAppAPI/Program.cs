using Infrastructure.Extensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

// Database - Converter DATABASE_URL do Render para formato Npgsql
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi encontrada.");

// Converter DATABASE_URL do Render (postgres://) para formato Npgsql
if (connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
}

// Adicionar connection string convertida à configuração
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// Database
builder.Services.AddInfrastructure(builder.Configuration);

// Static files (Angular)
builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "wwwroot";
});

var app = builder.Build();

// Porta dinâmica do Render (apenas em produção)
// Em desenvolvimento, usa a porta do launchSettings.json
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Clear();
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// ======================
// Pipeline
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

// 🔥 Migrations automáticas (executa ao iniciar a aplicação)
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
    // Isso permite que a aplicação inicie mesmo se houver problemas com o banco
}

app.Run();
