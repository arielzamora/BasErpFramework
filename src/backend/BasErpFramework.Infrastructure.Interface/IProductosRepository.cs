using BasErpFramework.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasErpFramework.Infrastructure.Interface;

public interface IProductosRepository
{
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<Producto?> GetAsync(string id);
    Task InsertAsync(Producto producto);
    Task UpdateAsync(Producto producto);
    Task DeleteAsync(Producto producto);
}
