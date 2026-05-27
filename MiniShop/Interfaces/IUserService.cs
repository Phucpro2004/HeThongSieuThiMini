using MiniShop.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> CreateUserAsync(UserCreateRequest request);
        Task<UserProfileResponse?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UserProfileUpdateRequest request);
    }
}
