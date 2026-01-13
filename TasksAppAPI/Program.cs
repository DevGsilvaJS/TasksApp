using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Services
// ======================

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Desenvolvimento: permitir tudo
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
    else
    {
        // Produção: permitir apenas origens específicas
        var corsOrigins = builder.Configuration["CorsOrigins"]?.Split(';') ?? Array.Empty<string>();
        options.AddPolicy("AllowAll", policy =>
        {
            if (corsOrigins.Length > 0)
            {
                policy
                    .WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
            else
            {
                // Se não configurado, permitir qualquer origem (não recomendado para produção)
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    }
});

// Infrastructure (PostgreSQL + EF + Repositórios)
// Usar variável de ambiente se disponível, senão usar appsettings
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi encontrada.");

// Converter DATABASE_URL do Render para formato Npgsql se necessário
if (connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
}

// Criar configuração temporária com connection string atualizada
var tempConfig = new Dictionary<string, string?>
{
    { "ConnectionStrings:DefaultConnection", connectionString }
};
builder.Configuration.AddInMemoryCollection(tempConfig);

builder.Services.AddInfrastructure(builder.Configuration);

// Servir arquivos estáticos do frontend
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// ======================
// Pipeline
// ======================

// 🔥 CORS SEMPRE ANTES DE TUDO
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Servir SPA Angular
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});

app.Run();
