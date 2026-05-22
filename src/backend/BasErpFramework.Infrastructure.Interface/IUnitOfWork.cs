using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BasErpFramework.Infrastructure.Interface
{
    public interface IUnitOfWork:IDisposable
    {
        IProductosRepository Productos { get; }

        IUsersRepository Users { get; }

        Task<int> CommitAsync();
    }
}
