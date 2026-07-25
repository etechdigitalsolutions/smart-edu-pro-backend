using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/timetable")]
public class TimetableController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public TimetableController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSlots([FromQuery] Guid? class_id, [FromQuery] Guid? teacher_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Timetable_Slot>().AsNoTracking().Where(t => t.Is_Active);
        if (class_id.HasValue) query = query.Where(t => t.Class_Id == class_id.Value);
        if (teacher_id.HasValue) query = query.Where(t => t.Teacher_Id == teacher_id.Value);

        var slots = await query.ToListAsync();
        return ApiOk(slots);
    }

    [HttpPost("slots")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotReq req)
    {
        var db = (DbContext)_context;
        var slot = new Timetable_Slot
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Branch_Id = req.Branch_Id ?? Guid.Empty,
            Class_Id = req.Class_Id,
            Subject_Id = req.Subject_Id,
            Teacher_Id = req.Teacher_Id,
            Day_Of_Week = Enum.TryParse<DayOfWeek>(req.Day_Of_Week, true, out var dow) ? dow : DayOfWeek.Monday,
            Start_Time = req.Start_Time,
            End_Time = req.End_Time,
            Room = req.Room ?? string.Empty,
            Effective_From = DateOnly.FromDateTime(DateTime.UtcNow),
            Is_Active = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Timetable_Slot>().Add(slot);
        await _context.SaveChangesAsync();

        return ApiCreated(slot, "Timetable slot created successfully");
    }

    [HttpPatch("slots/{id}")]
    public async Task<IActionResult> UpdateSlot(Guid id, [FromBody] UpdateSlotReq req)
    {
        var db = (DbContext)_context;
        var slot = await db.Set<Timetable_Slot>().FindAsync(id);
        if (slot == null) return ApiError("SLOT_NOT_FOUND", "Timetable slot not found", StatusCodes.Status404NotFound);

        if (req.Start_Time.HasValue) slot.Start_Time = req.Start_Time.Value;
        if (req.End_Time.HasValue) slot.End_Time = req.End_Time.Value;
        if (!string.IsNullOrWhiteSpace(req.Room)) slot.Room = req.Room;

        slot.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(slot, "Slot updated");
    }

    [HttpDelete("slots/{id}")]
    public async Task<IActionResult> DeleteSlot(Guid id)
    {
        var db = (DbContext)_context;
        var slot = await db.Set<Timetable_Slot>().FindAsync(id);
        if (slot == null) return ApiError("SLOT_NOT_FOUND", "Timetable slot not found", StatusCodes.Status404NotFound);

        slot.Is_Active = false;
        await _context.SaveChangesAsync();
        return ApiDeleted("Slot deactivated");
    }

    [HttpPost("overrides")]
    public async Task<IActionResult> CreateOverride([FromBody] CreateOverrideReq req)
    {
        var db = (DbContext)_context;
        var ov = new Timetable_Override
        {
            Id = Guid.NewGuid(),
            Time_Table_Slot_Id = req.Timetable_Slot_Id,
            Override_Date = req.Override_Date,
            Action = req.Action,
            New_Start_Time = req.New_Start_Time ?? TimeOnly.MinValue,
            New_End_Time = req.New_End_Time ?? TimeOnly.MinValue,
            New_Room = req.New_Room ?? string.Empty,
            Reason = req.Reason ?? string.Empty,
            Notify_Student = req.Notify_Students ?? true,
            Created_At = DateTime.UtcNow
        };

        db.Set<Timetable_Override>().Add(ov);
        await _context.SaveChangesAsync();

        return ApiCreated(ov, "Timetable override saved");
    }
}

public record CreateSlotReq(
    Guid Institute_Id,
    Guid? Branch_Id,
    Guid Class_Id,
    Guid Subject_Id,
    Guid Teacher_Id,
    string Day_Of_Week,
    TimeOnly Start_Time,
    TimeOnly End_Time,
    string? Room);

public record UpdateSlotReq(TimeOnly? Start_Time, TimeOnly? End_Time, string? Room);
public record CreateOverrideReq(
    Guid Timetable_Slot_Id,
    DateOnly Override_Date,
    string Action,
    TimeOnly? New_Start_Time,
    TimeOnly? New_End_Time,
    string? New_Room,
    string? Reason,
    bool? Notify_Students);
