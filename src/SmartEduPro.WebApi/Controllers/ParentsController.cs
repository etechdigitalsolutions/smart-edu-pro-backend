using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/parents")]
public class ParentsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ParentsController(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetParents([FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Parent>().AsNoTracking();
        var total = await query.CountAsync();
        var parents = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(parents, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateParent([FromBody] CreateParentReq req)
    {
        var db = (DbContext)_context;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            Phone = req.Phone,
            Password_Hash = _passwordHasher.HashPassword("Parent@123"),
            Role = User_Role.Parent,
            First_Name = req.First_Name,
            Last_Name = req.Last_Name,
            Is_Active = true,
            Is_Verified = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        db.Set<User>().Add(user);

        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            User_Id = user.Id,
            Institute_Id = req.Institute_Id,
            Occupation = req.Occupation ?? string.Empty,
            Whatsapp_No = req.Whatsapp_No ?? req.Phone,
            Address = req.Address ?? string.Empty,
            Preffered_Channel = Enum.TryParse<Notification_Channel>(req.Preferred_Channel, true, out var channel) ? channel : Notification_Channel.WhatsApp,
            Created_At = DateTime.UtcNow
        };
        db.Set<Parent>().Add(parent);

        await _context.SaveChangesAsync();

        return ApiCreated(parent, "Parent profile created");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetParent(Guid id)
    {
        var db = (DbContext)_context;
        var parent = await db.Set<Parent>().FirstOrDefaultAsync(p => p.Id == id);
        if (parent == null) return ApiError("PARENT_NOT_FOUND", "Parent not found", StatusCodes.Status404NotFound);
        return ApiOk(parent);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateParent(Guid id, [FromBody] UpdateParentReq req)
    {
        var db = (DbContext)_context;
        var parent = await db.Set<Parent>().FirstOrDefaultAsync(p => p.Id == id);
        if (parent == null) return ApiError("PARENT_NOT_FOUND", "Parent not found", StatusCodes.Status404NotFound);

        if (!string.IsNullOrWhiteSpace(req.Occupation)) parent.Occupation = req.Occupation;
        if (!string.IsNullOrWhiteSpace(req.Whatsapp_No)) parent.Whatsapp_No = req.Whatsapp_No;
        if (!string.IsNullOrWhiteSpace(req.Address)) parent.Address = req.Address;

        await _context.SaveChangesAsync();
        return ApiOk(parent, "Parent details updated");
    }

    [HttpPost("{id}/link-student")]
    public async Task<IActionResult> LinkStudent(Guid id, [FromBody] LinkStudentReq req)
    {
        var db = (DbContext)_context;
        var sp = new Student_Parent
        {
            Id = Guid.NewGuid(),
            Parent_Id = id,
            Student_Id = req.Student_Id,
            Relation = req.Relation,
            Is_Primary = req.Is_Primary ?? true,
            Created_At = DateTime.UtcNow
        };

        db.Set<Student_Parent>().Add(sp);
        await _context.SaveChangesAsync();

        return ApiCreated(sp, "Student linked to parent");
    }

    [HttpGet("{id}/children")]
    public async Task<IActionResult> GetChildren(Guid id)
    {
        var db = (DbContext)_context;
        var children = await db.Set<Student_Parent>()
            .Include(sp => sp.Students)
            .Where(sp => sp.Parent_Id == id)
            .Select(sp => sp.Students)
            .ToListAsync();

        return ApiOk(children);
    }
}

public record CreateParentReq(
    Guid Institute_Id,
    string First_Name,
    string Last_Name,
    string? Email,
    string Phone,
    string? Occupation,
    string? Whatsapp_No,
    string? Address,
    string? Preferred_Channel);

public record UpdateParentReq(
    string? Occupation,
    string? Whatsapp_No,
    string? Address);

public record LinkStudentReq(
    Guid Student_Id,
    string Relation,
    bool? Is_Primary);
