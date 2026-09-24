using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> UpdateUserRoleAsync(Guid userId, UserRole role);
}
