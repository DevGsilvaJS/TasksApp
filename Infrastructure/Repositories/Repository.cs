using System.Linq.Expressions;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> ListarTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> InserirAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task AtualizarAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task ExcluirAsync(T entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task ExcluirAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await ExcluirAsync(entity);
        }
    }

    public virtual async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public virtual async Task<T?> BuscarAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> BuscarTodosAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> ContarAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }
        return await _dbSet.CountAsync(predicate);
    }
}
