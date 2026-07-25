using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEduPro.Application.Features.Auth;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _sender.Send(command);
        return ApiCreated(result, "Registration successful. Check your phone for OTP verification.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _sender.Send(command);
        
        Response.Cookies.Append("smartedupro_rt", result.RefreshTokenStr, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        return ApiOk(new
        {
            access_token = result.Access_Token,
            token_type = result.Token_Type,
            expires_in = result.Expires_In,
            user = result.User
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("smartedupro_rt");
        return ApiOk(null, "Logged out successfully");
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _sender.Send(new GetMeQuery());
        return ApiOk(result);
    }
}
