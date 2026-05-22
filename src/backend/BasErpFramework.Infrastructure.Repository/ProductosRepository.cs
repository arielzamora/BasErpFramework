using BasErpFramework.Domain.Entity;
using BasErpFramework.Infrastructure.Data;
using BasErpFramework.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasErpFramework.Infrastructure.Repository;

public class ProductosRepository : IProductosRepository
{
    private readonly ApplicationDbContext _context;

    public ProductosRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Productos.ToListAsync();
    }

    public async Task<Producto?> GetAsync(string id)
    {
        return await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task InsertAsync(Producto producto)
    {
        await _context.Productos.AddAsync(producto);
    }

    public Task UpdateAsync(Producto producto)
    {
        _context.Productos.Update(producto);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Producto producto)
    {
        _context.Productos.Remove(producto);
        return Task.CompletedTask;
    }
}
