using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tarefa> Tarefas { get; set; }
    public DbSet<ImagemTarefa> ImagensTarefa { get; set; }
    public DbSet<AnotacaoTarefa> AnotacoesTarefas { get; set; }
    public DbSet<Anotacao> Anotacoes { get; set; }
    public DbSet<Duplicata> Duplicatas { get; set; }
    public DbSet<Parcela> Parcelas { get; set; }
    public DbSet<AnotacaoCliente> AnotacoesCliente { get; set; }
    public DbSet<Das> Das { get; set; }
    public DbSet<EnvioNotaServico> EnviosNotaServico { get; set; }
    public DbSet<PossivelCliente> PossiveisClientes { get; set; }
    public DbSet<PossivelClienteAnotacao> PossivelClienteAnotacoes { get; set; }
    public DbSet<CadastroStatusTarefa> CadastroStatusTarefa { get; set; }
    public DbSet<CadastroTipoAtendimento> CadastroTipoAtendimento { get; set; }
    public DbSet<CadastroTipoContato> CadastroTipoContato { get; set; }
    public DbSet<CadastroAndamento> CadastroAndamento { get; set; }
    public DbSet<CadastroStatusAtendimentoComercial> CadastroStatusAtendimentoComercial { get; set; }
    public DbSet<ClienteContratoValor> ClienteContratosValores { get; set; }
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<CentroCusto> CentrosCusto { get; set; }
    public DbSet<PlanoContas> PlanosContas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar enum StatusTarefa como inteiro no banco
        modelBuilder.Entity<Tarefa>()
            .Property(t => t.TarStatus)
            .HasConversion<int>();

        modelBuilder.Entity<Tarefa>()
            .Property(t => t.TarAndamento)
            .HasConversion<int>();

        // Configurar enum StatusDas como inteiro no banco
        modelBuilder.Entity<Das>()
            .Property(d => d.DasStatus)
            .HasConversion<int>();

        modelBuilder.Entity<CentroCusto>()
            .HasOne(c => c.Empresa)
            .WithMany(e => e.CentrosCusto)
            .HasForeignKey(c => c.EmpId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Duplicata>()
            .HasOne(d => d.CentroCusto)
            .WithMany()
            .HasForeignKey(d => d.CcuId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Duplicata>()
            .HasOne(d => d.PlanoContas)
            .WithMany()
            .HasForeignKey(d => d.PlcId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Parcela>()
            .HasOne(p => p.CentroCusto)
            .WithMany()
            .HasForeignKey(p => p.CcuId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Parcela>()
            .HasOne(p => p.PlanoContas)
            .WithMany()
            .HasForeignKey(p => p.PlcId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    public override int SaveChanges()
    {
        NormalizarStringsParaMaiusculo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizarStringsParaMaiusculo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void NormalizarStringsParaMaiusculo()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(string))
                    continue;

                if (property.CurrentValue is not string valor)
                    continue;

                var normalizado = valor.Trim();
                if (normalizado.Length == 0)
                {
                    property.CurrentValue = null;
                    continue;
                }

                property.CurrentValue = normalizado.ToUpperInvariant();
            }
        }
    }
}
