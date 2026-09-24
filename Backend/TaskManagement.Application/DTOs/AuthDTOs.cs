using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role = UserRole.User
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    DateTime Expiration,
    UserDto User
);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateUserRoleRequest(
    UserRole Role
);
