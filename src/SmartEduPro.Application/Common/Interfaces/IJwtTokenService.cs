using SmartEduPro.Domain.Entities;

namespace SmartEduPro.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, Guid? instituteId = null);
    string GenerateRefreshToken();
    string GenerateQrToken(Guid studentId, string secret);
    bool VerifyQrToken(string token, string secret, out Guid studentId);
}
