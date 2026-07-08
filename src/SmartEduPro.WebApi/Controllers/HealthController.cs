using Microsoft.AspNetCore.Mvc;

namespace SmartEduPro.WebApi.Controllers;

public class HealthController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Healthy", service = "SmartEduPro" });
    }
}
