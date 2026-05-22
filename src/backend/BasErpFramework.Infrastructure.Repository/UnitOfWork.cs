using BasErpFramework.Infrastructure.Interface;
using BasErpFramework.Infrastructure.Data;
using System.Threading.Tasks;
namespace BasErpFramework.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IProductosRepository Productos { get; }
        public IUsersRepository Users { get; }

        public UnitOfWork(ApplicationDbContext context, IProductosRepository productos, IUsersRepository users)
        {
            _context = context;
            Productos = productos;
            Users = users;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
            System.GC.SuppressFinalize(this);
        }
    }
}
