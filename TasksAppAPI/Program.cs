using Infrastructure.Extensions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TasksAppAPI.Scripts;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Services
// ======================
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensagens = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(e =>
                {
                    var msg = e.ErrorMessage;
                    var key = entry.Key;

                    if (string.IsNullOrWhiteSpace(msg))
                        return "Dados inválidos na requisição.";

                    if (msg.Contains("non-empty request body", StringComparison.OrdinalIgnoreCase)
                        || (key is "$" or "" && msg.Contains("required", StringComparison.OrdinalIgnoreCase))
                        || (key == "dto" && msg.Contains("required", StringComparison.OrdinalIgnoreCase)))
                    {
                        return "Não foram enviados dados na requisição.";
                    }

                    return msg;
                }))
                .Distinct()
                .ToList();

            var message = mensagens.FirstOrDefault() ?? "Requisição inválida. Verifique os dados enviados.";
            return new BadRequestObjectResult(new { message });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GA CONSULTORIA", Version = "v1" });
});

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

// Converter URL do Render (postgres:// ou postgresql://) para formato Npgsql
// Aceita ambos os formatos: postgres:// e postgresql://
if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
{
    try
    {
        // Normalizar para postgres:// se for postgresql:// (Uri precisa do formato correto)
        if (databaseUrl.StartsWith("postgresql://"))
        {
            databaseUrl = databaseUrl.Replace("postgresql://", "postgres://");
        }
        
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
// Migrations automáticas (ANTES de qualquer middleware)
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SincronizarHistoricoMigrationsRunner.Executar(db);
        Console.WriteLine("🔄 Aplicando migrations automaticamente...");
        try
        {
            db.Database.Migrate();
        }
        catch (PostgresException ex) when (ex.SqlState == "42P07")
        {
            // Tabela/objeto já existe (banco com histórico divergente de migrations).
            Console.WriteLine($"⚠️ Migração ignorada (objeto já existe): {ex.MessageText}");
        }
        catch (Exception ex) when (ex.InnerException is PostgresException pex && pex.SqlState == "42P07")
        {
            Console.WriteLine($"⚠️ Migração ignorada (objeto já existe): {pex.MessageText}");
        }
        // Garantir tabela de possíveis clientes (migration pode não estar no histórico)
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""TB_POSSIVEL_CLIENTE"" (
            ""POCID"" serial PRIMARY KEY,
            ""POCCODIGO"" character varying(50) NOT NULL,
            ""POCLOJA"" character varying(200) NULL,
            ""POCSTATUS"" character varying(100) NULL,
            ""POCFANTASIA"" character varying(300) NULL,
            ""POCDDD"" character varying(20) NULL,
            ""POCCNPJ"" character varying(20) NULL,
            ""POCRAZAOSOCIAL"" character varying(500) NULL,
            ""POCEMAILCOMERCIAL"" character varying(200) NULL,
            ""POCCELDDD"" character varying(20) NULL,
            ""POCCELULAR"" character varying(50) NULL,
            ""POCDATAIMPORTACAO"" timestamp with time zone NULL
        )");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_TB_POSSIVEL_CLIENTE_POCCODIGO"" ON ""TB_POSSIVEL_CLIENTE"" (""POCCODIGO"")");
        // Ampliar colunas se a tabela já existia com tamanhos menores
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ALTER COLUMN ""POCDDD"" TYPE character varying(20)");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ALTER COLUMN ""POCCELDDD"" TYPE character varying(20)");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ALTER COLUMN ""POCCELULAR"" TYPE character varying(50)");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_STATUS_ATENDIMENTO"" integer NULL");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_MOTIVO_PERDA"" character varying(500) NULL");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_POSSIVEL_CLIENTE"" ADD COLUMN IF NOT EXISTS ""POC_DATA_STATUS_ATENDIMENTO"" timestamp with time zone NULL");
        db.Database.ExecuteSqlRaw(@"ALTER TABLE ""TB_TAR_TAREFAS"" ADD COLUMN IF NOT EXISTS ""TARANDAMENTO"" integer NOT NULL DEFAULT 1");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""TB_CAD_ANDAMENTO"" (
            ""ANID"" integer NOT NULL,
            ""ANDESCRICAO"" character varying(100) NOT NULL,
            ""ANATIVO"" boolean NOT NULL DEFAULT true,
            CONSTRAINT ""PK_TB_CAD_ANDAMENTO"" PRIMARY KEY (""ANID"")
        )");
        db.Database.ExecuteSqlRaw(@"
            INSERT INTO ""TB_CAD_ANDAMENTO"" (""ANID"", ""ANDESCRICAO"", ""ANATIVO"") VALUES
            (1, 'A FAZER', true),
            (2, 'EM ANDAMENTO', true),
            (3, 'TESTAR', true),
            (4, 'RESOLVIDO', true)
            ON CONFLICT (""ANID"") DO NOTHING;
        ");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""TB_POSSIVEL_CLIENTE_ANOTACAO"" (
            ""PCAID"" serial PRIMARY KEY,
            ""POCID"" integer NOT NULL REFERENCES ""TB_POSSIVEL_CLIENTE""(""POCID""),
            ""USUID"" integer NOT NULL REFERENCES ""TB_USU_USUARIO""(""USUID""),
            ""PCADESCRICAO"" character varying(3000) NULL,
            ""PCADTCADASTRO"" timestamp with time zone NOT NULL
        )");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""TB_CAD_STATUS_ATEND_COMERCIAL"" (
            ""SACID"" serial PRIMARY KEY,
            ""SACNUMERO"" integer NOT NULL,
            ""SACDESCRICAO"" character varying(200) NOT NULL,
            ""SACATIVO"" boolean NOT NULL DEFAULT true
        )");
        if (!db.CadastroStatusAtendimentoComercial.Any())
        {
            var defaults = new[] {
                (1, "Não Iniciado"),
                (2, "Tentativa de Contato"),
                (3, "Contato Realizado"),
                (4, "Em Diagnóstico"),
                (5, "Proposta Enviada"),
                (6, "Em Negociação"),
                (7, "Follow-up"),
                (8, "Perdido"),
                (9, "Fechado / Ganho")
            };
            foreach (var (num, desc) in defaults)
            {
                db.CadastroStatusAtendimentoComercial.Add(new Domain.Entities.CadastroStatusAtendimentoComercial
                {
                    Numero = num,
                    Descricao = desc,
                    Ativo = true
                });
            }
            db.SaveChanges();
            Console.WriteLine("✅ Status de atendimento comercial (9 itens) inseridos.");
        }
        Console.WriteLine("✅ Migrations aplicadas com sucesso!");
    }
}

catch (Exception ex)
{
    Console.WriteLine($"❌ ERRO CRÍTICO ao aplicar migrations: {ex.Message}");
    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
    // Em produção, pode ser melhor falhar aqui para evitar problemas
    if (!app.Environment.IsDevelopment())
    {
        throw; // Falha o deploy se migrations não funcionarem em produção
    }
}

// ======================
// Ajuste único: se existir só 1 usuário e for Comercial (2), definir como Administrador (1)
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var usuarios = db.Usuarios.ToList();
        if (usuarios.Count == 1 && usuarios[0].UsuPerfil == 1)
        {
            usuarios[0].UsuPerfil = 2; // Administrador (2 = ver todos os módulos)
            db.SaveChanges();
            Console.WriteLine("✅ Único usuário definido como Administrador (perfil ajustado após migração).");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao ajustar perfil do único usuário: {ex.Message}");
}

// ======================
// Script: clientes que não devem aparecer na lista de comercial → marcar como inativos
// (lista de comercial filtra por POCSTATUS = '1 - OK')
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        const string sqlInativosComercial = """
            UPDATE "TB_POSSIVEL_CLIENTE"
            SET "POCSTATUS" = 'Inativo'
            WHERE "POCCODIGO" IN ('2494','3552','2489','4597','807','3844','4224','4146','2397')
            AND ("POCSTATUS" IS NULL OR "POCSTATUS" = '1 - OK')
            """;
        var rows = db.Database.ExecuteSqlRaw(sqlInativosComercial);
        if (rows > 0)
            Console.WriteLine($"✅ Script startup: {rows} possível(is) cliente(s) marcado(s) como inativo(s) na lista de comercial.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao executar script de inativos comercial: {ex.Message}");
}

// ======================
// Atualização CR (1x): vincular duplicatas CR aos clientes da lista (por código na descrição)
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("🔄 Executando atualização das CR para clientes configurados...");
        await AtualizacaoCrClientesRunner.ExecutarUmaVezAsync(db);
        Console.WriteLine("✅ Atualização CR concluída.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro na atualização CR: {ex.Message}");
    // Não interrompe a aplicação
}

// ======================
// Garantir tabela de contratos por cliente (sem depender de migrations)
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await GarantirTabelaClienteContratoValorRunner.ExecutarAsync(db);
        await GarantirColunaCodigoClienteTextoRunner.ExecutarAsync(db);
        await GarantirTabelasEmpresaCentroCustoPlanoContasRunner.ExecutarAsync(db);
        await GarantirColunaCentroCustoDuplicataRunner.ExecutarAsync(db);
        await GarantirColunaPlanoContasDuplicataRunner.ExecutarAsync(db);
        await GarantirColunaCentroCustoPlanoContasParcelaRunner.ExecutarAsync(db);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao garantir tabela TB_CLI_CONTRATO_VALOR: {ex.Message}");
}

// ======================
// Normalização global (opcional): converter strings existentes para MAIÚSCULO
// Habilite com variável de ambiente: NORMALIZAR_MAIUSCULO_STARTUP=true
// ======================
try
{
    var normalizar = Environment.GetEnvironmentVariable("NORMALIZAR_MAIUSCULO_STARTUP");
    if (string.Equals(normalizar, "true", StringComparison.OrdinalIgnoreCase))
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("🔄 Normalizando textos existentes para MAIÚSCULO (startup)...");
            await NormalizacaoMaiusculoRunner.ExecutarAsync(db);
            Console.WriteLine("✅ Normalização de MAIÚSCULO concluída.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro na normalização de MAIÚSCULO: {ex.Message}");
}

// ======================
// Importação da planilha de possíveis clientes (pasta Planilha)
// Em produção: /app/Planilha. Em dev: raiz do projeto (TasksApp/Planilha) ou TasksAppAPI/Planilha.
//// ======================
//try
//{
//    var baseDir = app.Environment.ContentRootPath ?? AppContext.BaseDirectory ?? ".";
//    var planilhaPath = Environment.GetEnvironmentVariable("PlanilhaPath");
//    if (string.IsNullOrWhiteSpace(planilhaPath))
//    {
//        var planilhaNoApp = Path.Combine(baseDir, "Planilha");
//        var planilhaNoPai = Path.GetFullPath(Path.Combine(baseDir, "..", "Planilha"));
//        planilhaPath = Directory.Exists(planilhaNoApp)
//            ? Path.GetFullPath(planilhaNoApp)
//            : planilhaNoPai;
//    }
//    else
//    {
//        planilhaPath = Path.GetFullPath(planilhaPath);
//    }
//    Console.WriteLine($"[Planilha] Caminho usado: {planilhaPath}");
//    using (var scope = app.Services.CreateScope())
//    {
//        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//        Console.WriteLine("🔄 Importando planilha de possíveis clientes...");
//        await PlanilhaPossiveisClientesRunner.ExecutarAsync(db, planilhaPath);
//        Console.WriteLine("✅ Importação da planilha concluída.");
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"⚠️ Erro na importação da planilha de possíveis clientes: {ex.Message}");
//}

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
// Seed de usuários padrão
// ======================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Verificar e criar usuário TI.GABRIEL
        var usuarioGabriel = db.Usuarios.FirstOrDefault(u => u.UsuLogin == "TI.GABRIEL");
        if (usuarioGabriel == null)
        {
            var pessoaGabriel = new Domain.Entities.Pessoa
            {
                PesFantasia = "Gabriel"
            };
            db.Pessoas.Add(pessoaGabriel);
            db.SaveChanges();
            
            var novoUsuarioGabriel = new Domain.Entities.Usuario
            {
                PesId = pessoaGabriel.PesId,
                UsuLogin = "TI.GABRIEL",
                UsuSenha = "1234GABRIEL",
                UsuPerfil = (int)Domain.Enums.PerfilUsuario.Administrador
            };
            db.Usuarios.Add(novoUsuarioGabriel);
            db.SaveChanges();
            Console.WriteLine("✅ Usuário TI.GABRIEL criado com sucesso!");
        }
        else if (usuarioGabriel.UsuPerfil == (int)Domain.Enums.PerfilUsuario.Comercial)
        {
            usuarioGabriel.UsuPerfil = (int)Domain.Enums.PerfilUsuario.Administrador;
            db.SaveChanges();
            Console.WriteLine("✅ Usuário TI.GABRIEL atualizado para Administrador.");
        }
        else
        {
            Console.WriteLine("ℹ️ Usuário TI.GABRIEL já existe.");
        }
        
        // Verificar e criar usuário TI.ABNER
        var usuarioAbner = db.Usuarios.FirstOrDefault(u => u.UsuLogin == "TI.ABNER");
        if (usuarioAbner == null)
        {
            var pessoaAbner = new Domain.Entities.Pessoa
            {
                PesFantasia = "Abner"
            };
            db.Pessoas.Add(pessoaAbner);
            db.SaveChanges();
            
            var novoUsuarioAbner = new Domain.Entities.Usuario
            {
                PesId = pessoaAbner.PesId,
                UsuLogin = "TI.ABNER",
                UsuSenha = "1234ABNER"
            };
            db.Usuarios.Add(novoUsuarioAbner);
            db.SaveChanges();
            Console.WriteLine("✅ Usuário TI.ABNER criado com sucesso!");
        }
        else
        {
            Console.WriteLine("ℹ️ Usuário TI.ABNER já existe.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao criar usuários padrão: {ex.Message}");
    // Não interrompe a aplicação se seed falhar
}

app.Run();
