using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/online-learning")]
public class OnlineLearningController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public OnlineLearningController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateLiveSession([FromBody] CreateLiveSessionReq req)
    {
        var db = (DbContext)_context;
        var roomId = $"room_{Guid.NewGuid():N}"[..16];
        var session = new Live_Session
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Class_Id = req.Class_Id,
            Subject_Id = req.Subject_Id,
            Teacher_Id = req.Teacher_Id,
            Title = req.Title,
            Scheduled_Start = req.Scheduled_Start,
            Scheduled_End = req.Scheduled_End ?? req.Scheduled_Start.AddHours(2),
            Status = Session_Status.Scheduled,
            Room_Id = roomId,
            Room_Token = $"token_{Guid.NewGuid():N}",
            Join_Url = $"https://live.smartedupro.lk/join/{roomId}",
            Chat_Enabled = req.Chat_Enabled ?? true,
            Recording_Enabled = req.Recording_Enabled ?? true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Live_Session>().Add(session);
        await _context.SaveChangesAsync();

        return ApiCreated(session, "Live class session scheduled");
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetLiveSessions([FromQuery] Guid? class_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Live_Session>().AsNoTracking();
        if (class_id.HasValue) query = query.Where(s => s.Class_Id == class_id.Value);

        var sessions = await query.ToListAsync();
        return ApiOk(sessions);
    }

    [HttpPost("sessions/{id}/join")]
    public async Task<IActionResult> JoinSession(Guid id, [FromBody] JoinSessionReq req)
    {
        var db = (DbContext)_context;
        var session = await db.Set<Live_Session>().FindAsync(id);
        if (session == null) return ApiError("SESSION_NOT_FOUND", "Live session not found", StatusCodes.Status404NotFound);

        session.Participant_Count += 1;
        if (session.Participant_Count > session.Peak_Viewers) session.Peak_Viewers = session.Participant_Count;

        var participant = new Live_Session_Participant
        {
            Id = Guid.NewGuid(),
            Session_Id = id,
            User_Id = req.User_Id,
            Role = req.Role,
            Joined_At = DateTime.UtcNow
        };

        db.Set<Live_Session_Participant>().Add(participant);
        await _context.SaveChangesAsync();

        return ApiOk(new
        {
            room_id = session.Room_Id,
            room_token = session.Room_Token,
            join_url = session.Join_Url
        }, "Joined live session");
    }

    [HttpPost("sessions/{id}/end")]
    public async Task<IActionResult> EndSession(Guid id)
    {
        var db = (DbContext)_context;
        var session = await db.Set<Live_Session>().FindAsync(id);
        if (session == null) return ApiError("SESSION_NOT_FOUND", "Live session not found", StatusCodes.Status404NotFound);

        session.Status = Session_Status.Ended;
        session.Actual_End = TimeOnly.FromDateTime(DateTime.UtcNow);
        session.Updated_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiOk(session, "Live session ended");
    }

    [HttpGet("recordings")]
    public async Task<IActionResult> GetRecordings([FromQuery] Guid? class_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Recording>().AsNoTracking().Where(r => r.Is_Published);
        if (class_id.HasValue) query = query.Where(r => r.Class_Id == class_id.Value);

        var recordings = await query.ToListAsync();
        return ApiOk(recordings);
    }

    [HttpPost("recordings")]
    public async Task<IActionResult> CreateRecording([FromBody] CreateRecordingReq req)
    {
        var db = (DbContext)_context;
        var recording = new Recording
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Session_Id = req.Session_Id ?? Guid.Empty,
            Class_Id = req.Class_Id ?? Guid.Empty,
            Subject_Id = req.Subject_Id ?? Guid.Empty,
            Teacher_Id = req.Teacher_Id ?? Guid.Empty,
            Title = req.Title,
            Description = req.Description ?? string.Empty,
            Duration_Sec = req.Duration_Secs ?? 0,
            File_Url = req.File_Url,
            Thumbnail_Url = req.Thumbnail_Url ?? string.Empty,
            Is_Published = true,
            Recorded_At = DateTime.UtcNow,
            Created_At = DateTime.UtcNow
        };

        db.Set<Recording>().Add(recording);
        await _context.SaveChangesAsync();

        return ApiCreated(recording, "Recording saved");
    }

    [HttpGet("resources")]
    public async Task<IActionResult> GetResources([FromQuery] Guid? class_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Learning_Resource>().AsNoTracking().Where(r => r.Is_Published);
        if (class_id.HasValue) query = query.Where(r => r.Class_Id == class_id.Value);

        var resources = await query.ToListAsync();
        return ApiOk(resources);
    }

    [HttpPost("resources")]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceReq req)
    {
        var db = (DbContext)_context;
        var res = new Learning_Resource
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Class_Id = req.Class_Id ?? Guid.Empty,
            Subject_Id = req.Subject_Id ?? Guid.Empty,
            Uploaded_By = req.Uploaded_By,
            Title = req.Title,
            Description = req.Description ?? string.Empty,
            File_Type = req.File_Type,
            File_Url = req.File_Url,
            File_Size_Mb = req.File_Size_Mb ?? 0,
            Is_Published = true,
            Created_At = DateTime.UtcNow
        };

        db.Set<Learning_Resource>().Add(res);
        await _context.SaveChangesAsync();

        return ApiCreated(res, "Learning resource uploaded");
    }
}

public record CreateLiveSessionReq(
    Guid Institute_Id,
    Guid Class_Id,
    Guid Subject_Id,
    Guid Teacher_Id,
    string Title,
    DateTime Scheduled_Start,
    DateTime? Scheduled_End,
    bool? Chat_Enabled,
    bool? Recording_Enabled);

public record JoinSessionReq(Guid User_Id, string Role);
public record CreateRecordingReq(
    Guid Institute_Id,
    Guid? Session_Id,
    Guid? Class_Id,
    Guid? Subject_Id,
    Guid? Teacher_Id,
    string Title,
    string? Description,
    int? Duration_Secs,
    string File_Url,
    string? Thumbnail_Url);

public record CreateResourceReq(
    Guid Institute_Id,
    Guid? Class_Id,
    Guid? Subject_Id,
    Guid Uploaded_By,
    string Title,
    string? Description,
    string File_Type,
    string File_Url,
    decimal? File_Size_Mb);
