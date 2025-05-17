using MilkChillar.Domain.Entities;

namespace MilkChillar.Infrastructure
{
    public interface ITokenService
    {
        string GenerateToken(User user, IList<string> roles, IList<string> permissions);
    }
}
