using Microsoft.AspNetCore.Mvc;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/health")]
public class HealthController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return ApiOk(new { status = "Healthy", service = "SmartEduPro" });
    }
}
