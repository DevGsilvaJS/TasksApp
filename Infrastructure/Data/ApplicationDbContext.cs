using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar enum StatusTarefa como inteiro no banco
        modelBuilder.Entity<Tarefa>()
            .Property(t => t.TarStatus)
            .HasConversion<int>();
    }
}
