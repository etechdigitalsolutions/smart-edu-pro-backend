using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idClaim, out var userId) ? userId : null;
        }
    }

    public User_Role? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<User_Role>(roleClaim, true, out var role) ? role : null;
        }
    }

    public Guid? InstituteId
    {
        get
        {
            var tenantHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Institute-ID"].ToString();
            if (Guid.TryParse(tenantHeader, out var headerTenantId))
                return headerTenantId;

            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("institute_id");
            return Guid.TryParse(claim, out var claimTenantId) ? claimTenantId : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
