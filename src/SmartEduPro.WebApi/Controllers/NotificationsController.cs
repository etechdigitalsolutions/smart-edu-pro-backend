using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/notifications")]
public class NotificationsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public NotificationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] Guid recipient_id, [FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Notification>().AsNoTracking()
            .Where(n => n.Recipient_Id == recipient_id)
            .OrderByDescending(n => n.Created_At);

        var total = await query.CountAsync();
        var notifications = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(notifications, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationReq req)
    {
        var db = (DbContext)_context;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id ?? Guid.Empty,
            Recipient_Id = req.Recipient_Id,
            Type = Enum.TryParse<Notification_Type>(req.Type, true, out var t) ? t : Notification_Type.Announcement,
            Title = req.Title,
            Body = req.Body,
            Channel = Enum.TryParse<Notification_Channel>(req.Channel, true, out var c) ? c : Notification_Channel.Push,
            Is_Read = false,
            Is_Delivered = true,
            Delivered_At = DateTime.UtcNow,
            Created_At = DateTime.UtcNow
        };

        db.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync();

        return ApiCreated(notification, "Notification sent");
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var db = (DbContext)_context;
        var n = await db.Set<Notification>().FindAsync(id);
        if (n != null)
        {
            n.Is_Read = true;
            n.Read_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return ApiOk(n, "Notification marked as read");
    }

    [HttpGet("announcements")]
    public async Task<IActionResult> GetAnnouncements([FromQuery] Guid institute_id)
    {
        var db = (DbContext)_context;
        var list = await db.Set<Announcement>().AsNoTracking()
            .Where(a => a.Institute_Id == institute_id)
            .OrderByDescending(a => a.Published_At)
            .ToListAsync();

        return ApiOk(list);
    }

    [HttpPost("announcements")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementReq req)
    {
        var db = (DbContext)_context;
        var anc = new Announcement
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Author_Id = req.Author_Id,
            Title = req.Title,
            Body = req.Body,
            Target_Class_Id = req.Target_Class_Id ?? Guid.Empty,
            Is_Pinned = req.Is_Pinned ?? false,
            Send_Push = req.Send_Push ?? true,
            Send_Sms = req.Send_Sms ?? false,
            Send_Whatsapp = req.Send_Whatsapp ?? false,
            Published_At = DateTime.UtcNow,
            Created_At = DateTime.UtcNow
        };

        db.Set<Announcement>().Add(anc);
        await _context.SaveChangesAsync();

        return ApiCreated(anc, "Announcement published successfully");
    }
}

public record SendNotificationReq(
    Guid? Institute_Id,
    Guid Recipient_Id,
    string Type,
    string Title,
    string Body,
    string Channel);

public record CreateAnnouncementReq(
    Guid Institute_Id,
    Guid Author_Id,
    string Title,
    string Body,
    Guid? Target_Class_Id,
    bool? Is_Pinned,
    bool? Send_Push,
    bool? Send_Sms,
    bool? Send_Whatsapp);
