namespace BasErpFramework.Infrastructure.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string customerId);
        Task<T> GetAsync(string customerId);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
