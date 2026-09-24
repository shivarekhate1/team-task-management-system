using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Exceptions;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("User identity not found in token.");
            }
            return userId;
        }
    }

    protected string CurrentUserRole
    {
        get
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
            {
                throw new UnauthorizedException("User role claim not found in token.");
            }
            return roleClaim;
        }
    }
}
