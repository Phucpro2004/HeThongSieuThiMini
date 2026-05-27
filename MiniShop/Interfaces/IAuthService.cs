using MiniShop.DTOs;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<string?> GoogleLoginAsync(string googleToken);
    }
}
