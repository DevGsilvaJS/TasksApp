using System.Linq.Expressions;

namespace Application.Interfaces;

public interface IRepository<T> where T : class
{
    // Operações CRUD básicas
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> ListarTodosAsync();
    Task<T> InserirAsync(T entity);
    Task AtualizarAsync(T entity);
    Task ExcluirAsync(T entity);
    Task ExcluirAsync(int id);
    Task<int> SalvarAlteracoesAsync();

    // Consultas com expressões
    Task<T?> BuscarAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> BuscarTodosAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate);
    Task<int> ContarAsync(Expression<Func<T, bool>>? predicate = null);
}
