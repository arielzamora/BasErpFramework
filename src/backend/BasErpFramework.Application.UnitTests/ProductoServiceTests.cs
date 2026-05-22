using AutoMapper;
using BasErpFramework.Application.Dto;
using BasErpFramework.Application.Interface;
using BasErpFramework.Application.Main;
using BasErpFramework.Application.Main.Hubs;
using BasErpFramework.Domain.Entity;
using BasErpFramework.Infrastructure.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BasErpFramework.Application.UnitTests;

public class ProductoServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductosRepository> _mockProductosRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductoService>> _mockLogger;
    private readonly Mock<IHubContext<ProductoHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly ProductoService _productoService;

    public ProductoServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductosRepo = new Mock<IProductosRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductoService>>();
        _mockHubContext = new Mock<IHubContext<ProductoHub>>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockTenantContext = new Mock<ITenantContext>();

        // Setup Tenant Context
        _mockTenantContext.Setup(t => t.TenantId).Returns("TestTenant");

        // Setup UnitOfWork
        _mockUnitOfWork.Setup(u => u.Productos).Returns(_mockProductosRepo.Object);

        // Setup SignalR HubContext
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _productoService = new ProductoService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockHubContext.Object,
            _mockTenantContext.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtoList()
    {
        // Arrange
        var productos = new List<Producto> { new Producto { Id = "1", Nombre = "A" } };
        var dtos = new List<ProductoDto> { new ProductoDto { Id = "1", Nombre = "A" } };

        _mockProductosRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(productos);
        _mockMapper.Setup(m => m.Map<IEnumerable<ProductoDto>>(productos)).Returns(dtos);

        // Act
        var result = await _productoService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("1", result.First().Id);
        _mockProductosRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InsertsAndNotifiesClients()
    {
        // Arrange
        var dtoToCreate = new ProductoDto { Nombre = "New Product", Precio = 100 };
        var productoEntity = new Producto { Nombre = "New Product", Precio = 100 };
        var createdDto = new ProductoDto { Id = "new-id", Nombre = "New Product", Precio = 100 };

        _mockMapper.Setup(m => m.Map<Producto>(dtoToCreate)).Returns(productoEntity);
        _mockMapper.Setup(m => m.Map<ProductoDto>(productoEntity)).Returns(createdDto);

        _mockProductosRepo.Setup(r => r.InsertAsync(It.IsAny<Producto>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1); // Assuming CommitAsync returns int

        // Act
        var result = await _productoService.CreateAsync(dtoToCreate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-id", result.Id);
        
        _mockProductosRepo.Verify(r => r.InsertAsync(It.IsAny<Producto>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        
        // Verify SignalR notification was sent
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("OnProductoUpdate", It.IsAny<object[]>(), default),
            Times.Once);
    }
}
