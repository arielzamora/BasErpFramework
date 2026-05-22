using BasErpFramework.Domain.Entity;

namespace BasErpFramework.Transversal.Common
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
