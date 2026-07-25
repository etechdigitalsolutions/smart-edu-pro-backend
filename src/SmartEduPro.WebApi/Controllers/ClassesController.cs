using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/classes")]
public class ClassesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public ClassesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetClasses([FromQuery] Guid? institute_id, [FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Class>().AsNoTracking().Where(c => c.Is_Active);
        if (institute_id.HasValue) query = query.Where(c => c.Institute_Id == institute_id.Value);

        var total = await query.CountAsync();
        var classes = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(classes, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassReq req)
    {
        var db = (DbContext)_context;
        var @class = new Class
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Branch_Id = req.Branch_Id ?? Guid.Empty,
            Name = req.Name,
            Code = req.Code ?? string.Empty,
            Study_Level = Enum.TryParse<Study_Level>(req.Study_Level, true, out var lvl) ? lvl : Study_Level.AL_Year_1,
            Al_Stream = Enum.TryParse<Al_Stream>(req.Al_Stream, true, out var stream) ? stream : Al_Stream.Science,
            Academic_year = req.Academic_Year,
            Class_Type = Enum.TryParse<Class_Type>(req.Class_Type, true, out var ct) ? ct : Class_Type.In_Person,
            Max_Student = req.Max_Students ?? 60,
            Description = req.Description ?? string.Empty,
            Is_Active = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Class>().Add(@class);
        await _context.SaveChangesAsync();

        return ApiCreated(@class, "Class created successfully");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClass(Guid id)
    {
        var db = (DbContext)_context;
        var @class = await db.Set<Class>().FindAsync(id);
        if (@class == null) return ApiError("CLASS_NOT_FOUND", "Class not found", StatusCodes.Status404NotFound);
        return ApiOk(@class);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] UpdateClassReq req)
    {
        var db = (DbContext)_context;
        var @class = await db.Set<Class>().FindAsync(id);
        if (@class == null) return ApiError("CLASS_NOT_FOUND", "Class not found", StatusCodes.Status404NotFound);

        if (!string.IsNullOrWhiteSpace(req.Name)) @class.Name = req.Name;
        if (req.Max_Students.HasValue) @class.Max_Student = req.Max_Students.Value;

        @class.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(@class, "Class updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClass(Guid id)
    {
        var db = (DbContext)_context;
        var @class = await db.Set<Class>().FindAsync(id);
        if (@class == null) return ApiError("CLASS_NOT_FOUND", "Class not found", StatusCodes.Status404NotFound);

        @class.Is_Active = false;
        @class.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiDeleted("Class deactivated");
    }

    [HttpPost("{id}/subjects")]
    public async Task<IActionResult> AddClassSubject(Guid id, [FromBody] AddClassSubjectReq req)
    {
        var db = (DbContext)_context;
        var cs = new Class_Subject
        {
            Id = Guid.NewGuid(),
            Class_Id = id,
            Subject_Id = req.Subject_Id,
            Teacher_Id = req.Teacher_Id ?? Guid.Empty,
            Monthly_Fee = req.Monthly_Fee ?? 0,
            Created_At = DateTime.UtcNow
        };

        db.Set<Class_Subject>().Add(cs);
        await _context.SaveChangesAsync();

        return ApiCreated(cs, "Subject assigned to class");
    }

    [HttpDelete("{id}/subjects/{subjectId}")]
    public async Task<IActionResult> RemoveClassSubject(Guid id, Guid subjectId)
    {
        var db = (DbContext)_context;
        var cs = await db.Set<Class_Subject>().FirstOrDefaultAsync(x => x.Class_Id == id && x.Subject_Id == subjectId);
        if (cs != null)
        {
            db.Set<Class_Subject>().Remove(cs);
            await _context.SaveChangesAsync();
        }

        return ApiDeleted("Subject removed from class");
    }

    [HttpGet("{id}/students")]
    public async Task<IActionResult> GetClassStudents(Guid id)
    {
        var db = (DbContext)_context;
        var studentIds = await db.Set<Student_Class_Enrollment>()
            .Where(e => e.Class_Id == id && e.Is_Active)
            .Select(e => e.Student_Id)
            .ToListAsync();

        var students = await db.Set<Student>()
            .Where(s => studentIds.Contains(s.Id))
            .ToListAsync();

        return ApiOk(students);
    }
}

public record CreateClassReq(
    Guid Institute_Id,
    Guid? Branch_Id,
    string Name,
    string? Code,
    string Study_Level,
    string? Al_Stream,
    int Academic_Year,
    string? Class_Type,
    int? Max_Students,
    string? Description);

public record UpdateClassReq(string? Name, int? Max_Students);
public record AddClassSubjectReq(Guid Subject_Id, Guid? Teacher_Id, decimal? Monthly_Fee);
