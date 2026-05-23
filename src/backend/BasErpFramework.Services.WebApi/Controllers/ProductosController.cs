using BasErpFramework.Application.Dto;
using BasErpFramework.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BasErpFramework.Services.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] ProductoDto productoDto)
    {
        if (productoDto == null) return BadRequest();
        var result = await _productoService.CreateAsync(productoDto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] ProductoDto productoDto)
    {
        if (productoDto == null) return BadRequest();
        try
        {
            var result = await _productoService.UpdateAsync(id, productoDto);
            return Ok(result);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var result = await _productoService.DeleteAsync(id);
        if (result) return Ok();
        return NotFound();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        var result = await _productoService.GetAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _productoService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("semantic-search")]
    public async Task<IActionResult> SemanticSearchAsync([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest();
        var result = await _productoService.SemanticSearchAsync(q);
        return Ok(result);
    }
}
