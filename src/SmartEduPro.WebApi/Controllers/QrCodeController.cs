using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/qr-code")]
public class QrCodeController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public QrCodeController(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentQr(Guid studentId)
    {
        var db = (DbContext)_context;
        var qrToken = await db.Set<Student_Qr_Token>().FirstOrDefaultAsync(t => t.Student_Id == studentId);
        if (qrToken == null) return ApiError("QR_NOT_FOUND", "QR Token not found for student", StatusCodes.Status404NotFound);

        return ApiOk(new
        {
            student_id = studentId,
            qr_token = qrToken.Token,
            qr_data = qrToken.Qr_Data,
            created_at = qrToken.Created_At
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyQr([FromBody] VerifyQrReq req)
    {
        var db = (DbContext)_context;
        var student = await db.Set<Student>().FindAsync(req.Student_Id);
        if (student == null) return ApiError("STUDENT_NOT_FOUND", "Student not found", StatusCodes.Status404NotFound);

        var valid = _jwtTokenService.VerifyQrToken(req.Qr_Token, student.Qr_Secret, out var tokenStudentId);
        return ApiOk(new
        {
            is_valid = valid && tokenStudentId == student.Id,
            student_id = student.Id,
            student_no = student.Student_No
        });
    }

    [HttpPost("regenerate/{studentId}")]
    public async Task<IActionResult> RegenerateQr(Guid studentId)
    {
        var db = (DbContext)_context;
        var student = await db.Set<Student>().FindAsync(studentId);
        if (student == null) return ApiError("STUDENT_NOT_FOUND", "Student not found", StatusCodes.Status404NotFound);

        student.Qr_Secret = Guid.NewGuid().ToString("N");

        var tokenRecord = await db.Set<Student_Qr_Token>().FirstOrDefaultAsync(t => t.Student_Id == studentId);
        var newTokenStr = _jwtTokenService.GenerateQrToken(student.Id, student.Qr_Secret);

        if (tokenRecord == null)
        {
            tokenRecord = new Student_Qr_Token
            {
                Id = Guid.NewGuid(),
                Student_Id = studentId,
                Token = newTokenStr,
                Qr_Data = newTokenStr,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            db.Set<Student_Qr_Token>().Add(tokenRecord);
        }
        else
        {
            tokenRecord.Token = newTokenStr;
            tokenRecord.Qr_Data = newTokenStr;
            tokenRecord.Updated_At = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ApiOk(new
        {
            student_id = studentId,
            qr_token = newTokenStr,
            qr_secret = student.Qr_Secret
        }, "QR Code regenerated successfully");
    }
}

public record VerifyQrReq(Guid Student_Id, string Qr_Token);
