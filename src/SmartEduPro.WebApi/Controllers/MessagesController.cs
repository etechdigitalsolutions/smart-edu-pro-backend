using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/messages")]
public class MessagesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public MessagesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("thread/{otherUserId}")]
    public async Task<IActionResult> GetThread(Guid otherUserId, [FromQuery] Guid current_user_id)
    {
        var db = (DbContext)_context;
        var messages = await db.Set<Message>().AsNoTracking()
            .Where(m => (m.Sender_Id == current_user_id && m.Recipient_Id == otherUserId) ||
                        (m.Sender_Id == otherUserId && m.Recipient_Id == current_user_id))
            .OrderBy(m => m.Created_At)
            .ToListAsync();

        return ApiOk(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageReq req)
    {
        var db = (DbContext)_context;
        var msg = new Message
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Sender_Id = req.Sender_Id,
            Recipient_Id = req.Recipient_Id,
            Body = req.Body,
            Attachment_Url = req.Attachment_Url,
            Is_Read = false,
            Created_At = DateTime.UtcNow
        };

        db.Set<Message>().Add(msg);
        await _context.SaveChangesAsync();

        return ApiCreated(msg, "Message sent");
    }

    [HttpPatch("read/{otherUserId}")]
    public async Task<IActionResult> MarkThreadAsRead(Guid otherUserId, [FromQuery] Guid current_user_id)
    {
        var db = (DbContext)_context;
        var unread = await db.Set<Message>()
            .Where(m => m.Sender_Id == otherUserId && m.Recipient_Id == current_user_id && !m.Is_Read)
            .ToListAsync();

        foreach (var m in unread)
        {
            m.Is_Read = true;
            m.Read_At = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ApiOk(null, "Thread marked as read");
    }
}

public record SendMessageReq(
    Guid Institute_Id,
    Guid Sender_Id,
    Guid Recipient_Id,
    string Body,
    string? Attachment_Url);
