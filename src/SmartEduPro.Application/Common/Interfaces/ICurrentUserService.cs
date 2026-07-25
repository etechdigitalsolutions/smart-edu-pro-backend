using SmartEduPro.Domain.Enums;

namespace SmartEduPro.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    User_Role? Role { get; }
    Guid? InstituteId { get; }
    bool IsAuthenticated { get; }
}
