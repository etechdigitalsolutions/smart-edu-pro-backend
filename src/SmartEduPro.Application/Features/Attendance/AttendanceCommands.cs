using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.Application.Features.Attendance;

public record ScanQrAttendanceCommand(Guid Session_Id, Guid Student_Id, string Qr_Token) : IRequest<object>;

public class ScanQrAttendanceCommandHandler : IRequestHandler<ScanQrAttendanceCommand, object>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public ScanQrAttendanceCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<object> Handle(ScanQrAttendanceCommand request, CancellationToken cancellationToken)
    {
        var db = (DbContext)_context;

        var student = await db.Set<Student>().FirstOrDefaultAsync(s => s.Id == request.Student_Id, cancellationToken);
        if (student == null) throw new KeyNotFoundException("STUDENT_NOT_FOUND: Student not found");

        var valid = _jwtTokenService.VerifyQrToken(request.Qr_Token, student.Qr_Secret, out var tokenStudentId);
        if (!valid || tokenStudentId != student.Id)
        {
            throw new InvalidOperationException("INVALID_QR_TOKEN: QR Code verification failed or expired");
        }

        var record = await db.Set<Attendance_Record>()
            .FirstOrDefaultAsync(r => r.Session_Id == request.Session_Id && r.Student_Id == student.Id, cancellationToken);

        if (record == null)
        {
            record = new Attendance_Record
            {
                Id = Guid.NewGuid(),
                Session_Id = request.Session_Id,
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

        var session = await db.Set<Attendance_Session>().FindAsync(new object[] { request.Session_Id }, cancellationToken);
        if (session != null) session.Total_Present += 1;

        await _context.SaveChangesAsync(cancellationToken);

        return new
        {
            student_id = student.Id,
            student_no = student.Student_No,
            status = record.Status.ToString(),
            check_in_time = record.Check_In_Time
        };
    }
}
