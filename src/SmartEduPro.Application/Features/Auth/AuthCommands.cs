using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.Application.Features.Auth;

public record RegisterCommand(
    string Email,
    string Phone,
    string Password,
    string First_Name,
    string Last_Name,
    User_Role Role,
    string? Institute_Name,
    string? Institute_City) : IRequest<RegisterResult>;

public record RegisterResult(object User, object? Institute);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var db = (DbContext)_context;
        var existingUser = await db.Set<User>().AnyAsync(u => u.Email == request.Email || u.Phone == request.Phone, cancellationToken);
        if (existingUser)
        {
            throw new InvalidOperationException("USER_ALREADY_EXISTS: User with given email or phone already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Phone = request.Phone,
            Password_Hash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            First_Name = request.First_Name,
            Last_Name = request.Last_Name,
            Is_Active = true,
            Is_Verified = false,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<User>().Add(user);

        Institute? institute = null;
        if (!string.IsNullOrWhiteSpace(request.Institute_Name))
        {
            institute = new Institute
            {
                Id = Guid.NewGuid(),
                Name = request.Institute_Name,
                Slug = request.Institute_Name.ToLower().Replace(" ", "-"),
                City = request.Institute_City ?? "Colombo",
                Phone = request.Phone ?? "+94000000000",
                Email = request.Email ?? "info@institute.lk",
                Subscription_Plan = Subscription_Plans.TRIAL,
                Subscription_Status = Subscription_Status.Trial,
                Trial_End = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                Is_Active = true,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            db.Set<Institute>().Add(institute);

            db.Set<Institute_Admin>().Add(new Institute_Admin
            {
                Id = Guid.NewGuid(),
                User_Id = user.Id,
                Institute_Id = institute.Id,
                Created_At = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResult(
            new
            {
                id = user.Id,
                email = user.Email,
                first_name = user.First_Name,
                last_name = user.Last_Name,
                role = user.Role.ToString(),
                is_verified = user.Is_Verified
            },
            institute != null ? new
            {
                id = institute.Id,
                name = institute.Name,
                slug = institute.Slug,
                subscription_plan = institute.Subscription_Plan.ToString(),
                trial_end = institute.Trial_End
            } : null
        );
    }
}

public record LoginCommand(string Identifier, string Password, object? Device_Info) : IRequest<LoginResult>;
public record LoginResult(string Access_Token, string Token_Type, int Expires_In, object User, string RefreshTokenStr);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var db = (DbContext)_context;
        var user = await db.Set<User>().FirstOrDefaultAsync(u => u.Email == request.Identifier || u.Phone == request.Identifier, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.Password_Hash))
        {
            throw new UnauthorizedAccessException("INVALID_CREDENTIALS: Invalid identifier or password");
        }

        var instituteAdmin = await db.Set<Institute_Admin>()
            .FirstOrDefaultAsync(ia => ia.User_Id == user.Id, cancellationToken);

        Guid? instituteId = instituteAdmin?.Institute_Id;
        string? instituteName = null;
        if (instituteId.HasValue)
        {
            var inst = await db.Set<Institute>().FindAsync(new object[] { instituteId.Value }, cancellationToken);
            instituteName = inst?.Name;
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user, instituteId);
        var refreshTokenStr = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new Refresh_Token
        {
            Id = Guid.NewGuid(),
            User_Id = user.Id,
            Token_Hash = _passwordHasher.HashPassword(refreshTokenStr),
            Ip_Address = IPAddress.Loopback,
            Expire_At = DateTime.UtcNow.AddDays(30),
            Created_At = DateTime.UtcNow
        };
        db.Set<Refresh_Token>().Add(refreshToken);

        user.Last_Login_At = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var userResult = new
        {
            id = user.Id,
            email = user.Email,
            phone = user.Phone,
            first_name = user.First_Name,
            last_name = user.Last_Name,
            role = user.Role.ToString(),
            avatar_url = user.Avatar_Url,
            institute_id = instituteId,
            institute_name = instituteName
        };

        return new LoginResult(accessToken, "Bearer", 900, userResult, refreshTokenStr);
    }
}

public record GetMeQuery : IRequest<object>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, object>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMeQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<object> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("UNAUTHORIZED: User not authenticated");

        var db = (DbContext)_context;
        var user = await db.Set<User>().FindAsync(new object[] { _currentUserService.UserId.Value }, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("USER_NOT_FOUND: User not found");

        return new
        {
            id = user.Id,
            email = user.Email,
            phone = user.Phone,
            first_name = user.First_Name,
            last_name = user.Last_Name,
            role = user.Role.ToString(),
            avatar_url = user.Avatar_Url,
            is_verified = user.Is_Verified,
            last_login_at = user.Last_Login_At
        };
    }
}
