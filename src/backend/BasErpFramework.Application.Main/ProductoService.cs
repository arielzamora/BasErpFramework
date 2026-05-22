using AutoMapper;
using BasErpFramework.Application.Dto;
using BasErpFramework.Application.Interface;
using BasErpFramework.Application.Main.Hubs;
using BasErpFramework.Domain.Entity;
using BasErpFramework.Infrastructure.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasErpFramework.Application.Main;

public class ProductoService : IProductoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductoService> _logger;
    private readonly IHubContext<ProductoHub> _hubContext;
    private readonly ITenantContext _tenantContext;

    public ProductoService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductoService> logger,
        IHubContext<ProductoHub> hubContext,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all productos for tenant {TenantId}", _tenantContext.TenantId);
        var productos = await _unitOfWork.Productos.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductoDto>>(productos);
    }

    public async Task<ProductoDto?> GetAsync(string id)
    {
        _logger.LogInformation("Getting producto {Id} for tenant {TenantId}", id, _tenantContext.TenantId);
        var producto = await _unitOfWork.Productos.GetAsync(id);
        if (producto == null) return null;
        return _mapper.Map<ProductoDto>(producto);
    }

    public async Task<ProductoDto> CreateAsync(ProductoDto productoDto)
    {
        _logger.LogInformation("Creating new producto {Nombre} for tenant {TenantId}", productoDto.Nombre, _tenantContext.TenantId);
        var producto = _mapper.Map<Producto>(productoDto);
        producto.Id = Guid.NewGuid().ToString(); // Ensure a new ID is generated
        
        await _unitOfWork.Productos.InsertAsync(producto);
        await _unitOfWork.CommitAsync();

        var result = _mapper.Map<ProductoDto>(producto);
        await NotifyClientsAsync("ProductoCreated", result);
        return result;
    }

    public async Task<ProductoDto> UpdateAsync(string id, ProductoDto productoDto)
    {
        _logger.LogInformation("Updating producto {Id} for tenant {TenantId}", id, _tenantContext.TenantId);
        var producto = await _unitOfWork.Productos.GetAsync(id);
        if (producto == null)
        {
            _logger.LogWarning("Producto {Id} not found for update in tenant {TenantId}", id, _tenantContext.TenantId);
            throw new Exception("Producto not found");
        }

        producto.Codigo = productoDto.Codigo;
        producto.Nombre = productoDto.Nombre;
        producto.Precio = productoDto.Precio;

        await _unitOfWork.Productos.UpdateAsync(producto);
        await _unitOfWork.CommitAsync();

        var result = _mapper.Map<ProductoDto>(producto);
        await NotifyClientsAsync("ProductoUpdated", result);
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting producto {Id} for tenant {TenantId}", id, _tenantContext.TenantId);
        var producto = await _unitOfWork.Productos.GetAsync(id);
        if (producto == null)
        {
            _logger.LogWarning("Producto {Id} not found for deletion in tenant {TenantId}", id, _tenantContext.TenantId);
            return false;
        }

        await _unitOfWork.Productos.DeleteAsync(producto);
        await _unitOfWork.CommitAsync();

        await NotifyClientsAsync("ProductoDeleted", id);
        return true;
    }

    public async Task<IEnumerable<ProductoDto>> SemanticSearchAsync(string query)
    {
        _logger.LogInformation("Performing semantic search for '{Query}' in tenant {TenantId}", query, _tenantContext.TenantId);
        // Simulate semantic search by returning products matching the query loosely
        var productos = await _unitOfWork.Productos.GetAllAsync();
        var lowerQuery = query.ToLower();
        var filtered = productos.Where(p => p.Nombre.ToLower().Contains(lowerQuery) || p.Codigo.ToLower().Contains(lowerQuery));
        return _mapper.Map<IEnumerable<ProductoDto>>(filtered);
    }

    private async Task NotifyClientsAsync(string action, object payload)
    {
        var tenantGroup = $"Tenant_{_tenantContext.TenantId}";
        _logger.LogInformation("Notifying SignalR group {TenantGroup} about {Action}", tenantGroup, action);
        await _hubContext.Clients.Group(tenantGroup).SendAsync("OnProductoUpdate", new { Action = action, Payload = payload });
    }
}
