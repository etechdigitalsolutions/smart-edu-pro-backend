using Microsoft.AspNetCore.Mvc;

namespace SmartEduPro.WebApi.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ApiOk(object data, string? message = null, object? meta = null)
        => Ok(new { success = true, data, message, meta });

    protected IActionResult ApiCreated(object data, string? message = null, object? meta = null)
        => Created(string.Empty, new { success = true, data, message, meta });

    protected IActionResult ApiDeleted(string message)
        => Ok(new { success = true, message });

    protected IActionResult ApiError(string code, string message, int statusCode = StatusCodes.Status400BadRequest, string? field = null, object? details = null)
        => StatusCode(statusCode, new
        {
            success = false,
            error = new { code, message, field, details },
            meta = new { timestamp = DateTime.UtcNow }
        });

    protected static string NewId(string prefix) => $"{prefix}_{Guid.NewGuid():N}"[..(prefix.Length + 9)];
}
