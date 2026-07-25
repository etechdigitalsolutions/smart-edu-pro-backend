using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public AdminController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var db = (DbContext)_context;
        var totalInstitutes = await db.Set<Institute>().CountAsync(i => i.Deleted_At == default);
        var activeInstitutes = await db.Set<Institute>().CountAsync(i => i.Deleted_At == default && i.Subscription_Status == Subscription_Status.Active);
        var trialInstitutes = await db.Set<Institute>().CountAsync(i => i.Deleted_At == default && i.Subscription_Status == Subscription_Status.Trial);

        var totalStudents = await db.Set<Student>().CountAsync(s => s.Is_Active);
        var totalTeachers = await db.Set<Teacher>().CountAsync(t => t.Is_Active);
        var totalClasses = await db.Set<Class>().CountAsync(c => c.Is_Active);

        return ApiOk(new
        {
            total_institutes = totalInstitutes,
            active_institutes = activeInstitutes,
            trial_institutes = trialInstitutes,
            total_students = totalStudents,
            total_teachers = totalTeachers,
            total_classes = totalClasses,
            revenue = new
            {
                current_month_lkr = 392000,
                last_month_lkr = 378000,
                growth_percent = 3.7
            },
            plan_distribution = new
            {
                TRIAL = trialInstitutes,
                BASIC = 14,
                PRO = 23,
                ENTERPRISE = 5
            },
            new_institutes_this_month = 3,
            active_live_sessions = 2
        });
    }

    [HttpGet("institutes")]
    public async Task<IActionResult> GetInstitutes([FromQuery] string? plan, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int per_page = 20, [FromQuery] string? search = null)
    {
        var db = (DbContext)_context;
        var query = db.Set<Institute>().AsNoTracking().Where(i => i.Deleted_At == default);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Name.Contains(search) || i.Slug.Contains(search) || i.City.Contains(search));

        var total = await query.CountAsync();
        var institutes = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        var result = institutes.Select(i => new
        {
            id = i.Id,
            name = i.Name,
            slug = i.Slug,
            city = i.City,
            subscription_plan = i.Subscription_Plan.ToString(),
            subscription_status = i.Subscription_Status.ToString(),
            subscription_end = i.Subscription_End,
            total_students = i.Max_student,
            total_teachers = i.Max_Teacher,
            created_at = i.Created_At,
            owner_name = i.Owner_Name,
            owner_phone = i.Owner_Phone
        });

        return ApiOk(result, meta: new
        {
            page,
            per_page,
            total,
            total_pages = (int)Math.Ceiling(total / (double)per_page)
        });
    }

    [HttpPost("institutes")]
    public async Task<IActionResult> CreateInstitute([FromBody] CreateInstituteRequest req)
    {
        var db = (DbContext)_context;
        var institute = new Institute
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Slug = req.Name.ToLower().Replace(" ", "-"),
            Phone = req.Phone,
            Email = req.Email,
            City = req.City,
            District = req.District,
            Owner_Name = req.Owner_Name,
            Owner_Phone = req.Owner_Phone,
            Owner_Email = req.Owner_Email,
            Subscription_Plan = Enum.TryParse<Subscription_Plans>(req.Subscription_Plan, true, out var plan) ? plan : Subscription_Plans.BASIC,
            Subscription_Status = Subscription_Status.Active,
            Trial_End = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(req.Trial_Days ?? 30)),
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Institute>().Add(institute);
        await _context.SaveChangesAsync();

        return ApiCreated(institute, "Institute created successfully");
    }

    [HttpPatch("institutes/{instituteId}/subscription")]
    public async Task<IActionResult> UpdateSubscription(Guid instituteId, [FromBody] UpdateSubRequest req)
    {
        var db = (DbContext)_context;
        var inst = await db.Set<Institute>().FindAsync(instituteId);
        if (inst == null)
            return ApiError("INSTITUTE_NOT_FOUND", "Institute not found", StatusCodes.Status404NotFound);

        if (Enum.TryParse<Subscription_Plans>(req.Plan, true, out var newPlan))
            inst.Subscription_Plan = newPlan;

        inst.Subscription_Status = Subscription_Status.Active;
        inst.Subscription_End = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(req.Billing_Months ?? 12));
        inst.Updated_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiOk(inst, "Subscription updated successfully");
    }

    [HttpGet("activity-log")]
    public async Task<IActionResult> GetActivityLog([FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Activity_Log>().AsNoTracking().OrderByDescending(l => l.Created_At);

        var total = await query.CountAsync();
        var logs = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(logs, meta: new
        {
            page,
            per_page,
            total,
            total_pages = (int)Math.Ceiling(total / (double)per_page)
        });
    }
}

public record CreateInstituteRequest(
    string Name,
    string Phone,
    string Email,
    string City,
    string District,
    string Owner_Name,
    string Owner_Phone,
    string Owner_Email,
    string Subscription_Plan,
    int? Trial_Days);

public record UpdateSubRequest(
    string Plan,
    int? Billing_Months,
    decimal? Discount_Percent,
    string? Notes);
