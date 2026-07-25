using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEduPro.Application.Features.Students;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/students")]
public class StudentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] int page = 1, [FromQuery] int per_page = 20, [FromQuery] string? search = null)
    {
        var result = await _sender.Send(new GetStudentsQuery(page, per_page, search));
        return ApiOk(result.Data, meta: new
        {
            page = result.Page,
            per_page = result.PerPage,
            total = result.Total,
            total_pages = (int)Math.Ceiling(result.Total / (double)result.PerPage)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentCommand command)
    {
        var result = await _sender.Send(command);
        return ApiCreated(result, "Student registered successfully");
    }
}
