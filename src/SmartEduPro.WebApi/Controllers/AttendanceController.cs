using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/attendance")]
public class AttendanceController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AttendanceController(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAttSessionReq req)
    {
        var db = (DbContext)_context;
        var session = new Attendance_Session
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Class_Id = req.Class_Id,
            Subject_Id = req.Subject_Id,
            Teacher_Id = req.Teacher_Id,
            Session_Date = req.Session_Date,
            Start_Time = req.Start_Time,
            End_Time = req.End_Time ?? req.Start_Time.AddHours(2),
            Is_Completed = false,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Attendance_Session>().Add(session);
        await _context.SaveChangesAsync();

        return ApiCreated(session, "Attendance session created");
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] Guid? class_id, [FromQuery] DateOnly? date)
    {
        var db = (DbContext)_context;
        var query = db.Set<Attendance_Session>().AsNoTracking();
        if (class_id.HasValue) query = query.Where(s => s.Class_Id == class_id.Value);
        if (date.HasValue) query = query.Where(s => s.Session_Date == date.Value);

        var sessions = await query.ToListAsync();
        return ApiOk(sessions);
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var db = (DbContext)_context;
        var session = await db.Set<Attendance_Session>().FindAsync(id);
        if (session == null) return ApiError("SESSION_NOT_FOUND", "Attendance session not found", StatusCodes.Status404NotFound);

        var records = await db.Set<Attendance_Record>()
            .Where(r => r.Session_Id == id)
            .ToListAsync();

        return ApiOk(new { session, records });
    }

    [HttpPost("scan")]
    public async Task<IActionResult> ScanQr([FromBody] QrScanReq req)
    {
        var db = (DbContext)_context;

        var student = await db.Set<Student>().FirstOrDefaultAsync(s => s.Id == req.Student_Id);
        if (student == null) return ApiError("STUDENT_NOT_FOUND", "Student not found", StatusCodes.Status404NotFound);

        var valid = _jwtTokenService.VerifyQrToken(req.Qr_Token, student.Qr_Secret, out var tokenStudentId);
        if (!valid || tokenStudentId != student.Id)
        {
            return ApiError("INVALID_QR_TOKEN", "QR Code verification failed or expired");
        }

        var record = await db.Set<Attendance_Record>()
            .FirstOrDefaultAsync(r => r.Session_Id == req.Session_Id && r.Student_Id == student.Id);

        if (record == null)
        {
            record = new Attendance_Record
            {
                Id = Guid.NewGuid(),
                Session_Id = req.Session_Id,
                Student_Id = student.Id,
                Status = Attendance_Status.Present,
                Check_In_Time = DateTime.UtcNow,
                Marked_Via = "QR",
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            db.Set<Attendance_Record>().Add(record);
        }
        else
        {
            record.Status = Attendance_Status.Present;
            record.Check_In_Time = DateTime.UtcNow;
            record.Updated_At = DateTime.UtcNow;
        }

        var session = await db.Set<Attendance_Session>().FindAsync(req.Session_Id);
        if (session != null) session.Total_Present += 1;

        await _context.SaveChangesAsync();

        return ApiOk(new
        {
            student_id = student.Id,
            student_no = student.Student_No,
            status = record.Status.ToString(),
            check_in_time = record.Check_In_Time
        }, "Attendance marked successfully");
    }

    [HttpPost("manual")]
    public async Task<IActionResult> ManualAttendance([FromBody] ManualAttReq req)
    {
        var db = (DbContext)_context;
        var record = await db.Set<Attendance_Record>()
            .FirstOrDefaultAsync(r => r.Session_Id == req.Session_Id && r.Student_Id == req.Student_Id);

        if (record == null)
        {
            record = new Attendance_Record
            {
                Id = Guid.NewGuid(),
                Session_Id = req.Session_Id,
                Student_Id = req.Student_Id,
                Status = Enum.TryParse<Attendance_Status>(req.Status, true, out var st) ? st : Attendance_Status.Present,
                Check_In_Time = DateTime.UtcNow,
                Marked_Via = "MANUAL",
                Text = req.Notes ?? string.Empty,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            db.Set<Attendance_Record>().Add(record);
        }
        else
        {
            if (Enum.TryParse<Attendance_Status>(req.Status, true, out var st)) record.Status = st;
            record.Text = req.Notes ?? string.Empty;
            record.Updated_At = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ApiOk(record, "Attendance updated");
    }

    [HttpPost("sessions/{id}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id)
    {
        var db = (DbContext)_context;
        var session = await db.Set<Attendance_Session>().FindAsync(id);
        if (session == null) return ApiError("SESSION_NOT_FOUND", "Session not found", StatusCodes.Status404NotFound);

        session.Is_Completed = true;
        session.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(session, "Attendance session completed");
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var db = (DbContext)_context;

        var totalSessions = await db.Set<Attendance_Session>().CountAsync(s => s.Session_Date == targetDate);
        var totalPresent = await db.Set<Attendance_Record>().CountAsync(r => r.Attendance_Sessions.Session_Date == targetDate && r.Status == Attendance_Status.Present);
        var totalAbsent = await db.Set<Attendance_Record>().CountAsync(r => r.Attendance_Sessions.Session_Date == targetDate && r.Status == Attendance_Status.Absent);

        return ApiOk(new
        {
            date = targetDate,
            total_sessions = totalSessions,
            total_present = totalPresent,
            total_absent = totalAbsent
        });
    }
}

public record CreateAttSessionReq(
    Guid Institute_Id,
    Guid Class_Id,
    Guid Subject_Id,
    Guid Teacher_Id,
    DateOnly Session_Date,
    TimeOnly Start_Time,
    TimeOnly? End_Time);

public record QrScanReq(Guid Session_Id, Guid Student_Id, string Qr_Token);
public record ManualAttReq(Guid Session_Id, Guid Student_Id, string Status, string? Notes);
