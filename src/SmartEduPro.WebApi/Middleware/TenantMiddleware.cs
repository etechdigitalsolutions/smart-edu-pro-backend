namespace SmartEduPro.WebApi.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract multi-tenant institute header
        if (context.Request.Headers.TryGetValue("X-Institute-ID", out var instituteHeader))
        {
            context.Items["InstituteId"] = instituteHeader.ToString();
        }

        await _next(context);
    }
}
