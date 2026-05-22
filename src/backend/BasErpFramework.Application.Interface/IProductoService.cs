using BasErpFramework.Application.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasErpFramework.Application.Interface;

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync();
    Task<ProductoDto?> GetAsync(string id);
    Task<ProductoDto> CreateAsync(ProductoDto productoDto);
    Task<ProductoDto> UpdateAsync(string id, ProductoDto productoDto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<ProductoDto>> SemanticSearchAsync(string query);
}
