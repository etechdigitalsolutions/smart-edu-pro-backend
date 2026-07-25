using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/teachers")]
public class TeachersController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public TeachersController(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachers([FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Teacher>().AsNoTracking().Where(t => t.Is_Active);
        var total = await query.CountAsync();
        var teachers = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(teachers, meta: new
        {
            page,
            per_page,
            total,
            total_pages = (int)Math.Ceiling(total / (double)per_page)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherReq req)
    {
        var db = (DbContext)_context;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            Phone = req.Phone,
            Password_Hash = _passwordHasher.HashPassword("Teacher@123"),
            Role = User_Role.Teacher,
            First_Name = req.First_Name,
            Last_Name = req.Last_Name,
            Is_Active = true,
            Is_Verified = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        db.Set<User>().Add(user);

        var count = await db.Set<Teacher>().CountAsync(t => t.Institute_Id == req.Institute_Id);
        var teacherNo = $"TCH-{(count + 1):D3}";

        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            User_Id = user.Id,
            Institute_Id = req.Institute_Id,
            Teacher_No = teacherNo,
            Nic = req.Nic ?? string.Empty,
            Qualification = req.Qualification ?? string.Empty,
            Specialization = req.Specialization ?? string.Empty,
            Experience_Yrs = req.Experience_Yrs ?? 0,
            Joined_Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Is_Active = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        db.Set<Teacher>().Add(teacher);

        await _context.SaveChangesAsync();

        return ApiCreated(teacher, "Teacher created successfully");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacher(Guid id)
    {
        var db = (DbContext)_context;
        var teacher = await db.Set<Teacher>().FirstOrDefaultAsync(t => t.Id == id);
        if (teacher == null) return ApiError("TEACHER_NOT_FOUND", "Teacher not found", StatusCodes.Status404NotFound);
        return ApiOk(teacher);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTeacher(Guid id, [FromBody] UpdateTeacherReq req)
    {
        var db = (DbContext)_context;
        var teacher = await db.Set<Teacher>().FirstOrDefaultAsync(t => t.Id == id);
        if (teacher == null) return ApiError("TEACHER_NOT_FOUND", "Teacher not found", StatusCodes.Status404NotFound);

        var user = await db.Set<User>().FindAsync(teacher.User_Id);
        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(req.First_Name)) user.First_Name = req.First_Name;
            if (!string.IsNullOrWhiteSpace(req.Last_Name)) user.Last_Name = req.Last_Name;
        }

        if (!string.IsNullOrWhiteSpace(req.Qualification)) teacher.Qualification = req.Qualification;
        if (!string.IsNullOrWhiteSpace(req.Specialization)) teacher.Specialization = req.Specialization;

        teacher.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(teacher, "Teacher details updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeacher(Guid id)
    {
        var db = (DbContext)_context;
        var teacher = await db.Set<Teacher>().FindAsync(id);
        if (teacher == null) return ApiError("TEACHER_NOT_FOUND", "Teacher not found", StatusCodes.Status404NotFound);

        teacher.Is_Active = false;
        teacher.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiDeleted("Teacher deactivated successfully");
    }

    [HttpPost("{id}/assign-class")]
    public async Task<IActionResult> AssignClass(Guid id, [FromBody] AssignClassReq req)
    {
        var db = (DbContext)_context;
        var assignment = new Teacher_Class_Assignment
        {
            Id = Guid.NewGuid(),
            Teacher_Id = id,
            Class_Id = req.Class_Id,
            Subject_Id = req.Subject_Id,
            Is_Primary = req.Is_Primary ?? true,
            From_Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Created_At = DateTime.UtcNow
        };

        db.Set<Teacher_Class_Assignment>().Add(assignment);
        await _context.SaveChangesAsync();

        return ApiCreated(assignment, "Teacher assigned to class successfully");
    }

    [HttpDelete("{id}/assignments/{assignmentId}")]
    public async Task<IActionResult> RemoveAssignment(Guid id, Guid assignmentId)
    {
        var db = (DbContext)_context;
        var assignment = await db.Set<Teacher_Class_Assignment>().FindAsync(assignmentId);
        if (assignment != null)
        {
            db.Set<Teacher_Class_Assignment>().Remove(assignment);
            await _context.SaveChangesAsync();
        }
        return ApiDeleted("Assignment removed");
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetTeacherSchedule(Guid id)
    {
        var db = (DbContext)_context;
        var slots = await db.Set<Timetable_Slot>()
            .Where(s => s.Teacher_Id == id && s.Is_Active)
            .ToListAsync();

        return ApiOk(slots);
    }
}

public record CreateTeacherReq(
    Guid Institute_Id,
    string First_Name,
    string Last_Name,
    string Email,
    string Phone,
    string? Nic,
    string? Qualification,
    string? Specialization,
    int? Experience_Yrs);

public record UpdateTeacherReq(
    string? First_Name,
    string? Last_Name,
    string? Qualification,
    string? Specialization);

public record AssignClassReq(
    Guid Class_Id,
    Guid Subject_Id,
    bool? Is_Primary);
