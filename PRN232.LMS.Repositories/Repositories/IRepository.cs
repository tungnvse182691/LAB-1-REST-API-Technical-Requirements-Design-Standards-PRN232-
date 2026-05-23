namespace PRN232.LMS.Repositories.Repositories;

/// <summary>
/// Generic data-access contract. No business logic, no sorting, no filtering.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
