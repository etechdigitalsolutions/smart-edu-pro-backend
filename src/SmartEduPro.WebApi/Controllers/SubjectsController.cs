using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/subjects")]
public class SubjectsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public SubjectsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects([FromQuery] Guid? institute_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Subject>().AsNoTracking().Where(s => s.Is_Active);
        if (institute_id.HasValue) query = query.Where(s => s.Insitute_Id == institute_id.Value);

        var subjects = await query.ToListAsync();
        return ApiOk(subjects);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectReq req)
    {
        var db = (DbContext)_context;
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Insitute_Id = req.Institute_Id,
            Name = req.Name,
            Code = req.Code ?? string.Empty,
            Study_Level = Enum.TryParse<Study_Level>(req.Study_Level, true, out var lvl) ? lvl : Study_Level.AL_Year_1,
            Al_Stream = Enum.TryParse<Al_Stream>(req.Al_Stream, true, out var stream) ? stream : Al_Stream.Science,
            Description = req.Description ?? string.Empty,
            Is_Active = true,
            Created_At = DateTime.UtcNow
        };

        db.Set<Subject>().Add(subject);
        await _context.SaveChangesAsync();

        return ApiCreated(subject, "Subject created successfully");
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectReq req)
    {
        var db = (DbContext)_context;
        var subject = await db.Set<Subject>().FindAsync(id);
        if (subject == null) return ApiError("SUBJECT_NOT_FOUND", "Subject not found", StatusCodes.Status404NotFound);

        if (!string.IsNullOrWhiteSpace(req.Name)) subject.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Description)) subject.Description = req.Description;

        await _context.SaveChangesAsync();
        return ApiOk(subject, "Subject updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var db = (DbContext)_context;
        var subject = await db.Set<Subject>().FindAsync(id);
        if (subject == null) return ApiError("SUBJECT_NOT_FOUND", "Subject not found", StatusCodes.Status404NotFound);

        subject.Is_Active = false;
        await _context.SaveChangesAsync();
        return ApiDeleted("Subject deactivated");
    }
}

public record CreateSubjectReq(
    Guid Institute_Id,
    string Name,
    string? Code,
    string Study_Level,
    string? Al_Stream,
    string? Description);

public record UpdateSubjectReq(string? Name, string? Description);
