using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.Application.Features.Students;

public record GetStudentsQuery(int Page = 1, int Per_Page = 20, string? Search = null) : IRequest<GetStudentsResult>;
public record GetStudentsResult(IEnumerable<object> Data, int Total, int Page, int PerPage);

public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, GetStudentsResult>
{
    private readonly IApplicationDbContext _context;

    public GetStudentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetStudentsResult> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        var db = (DbContext)_context;
        var query = db.Set<Student>()
            .Include(s => s.Users)
            .AsNoTracking()
            .Where(s => s.Is_Active);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.Student_No.Contains(request.Search) ||
                                     s.Users.First_Name.Contains(request.Search) ||
                                     s.Users.Last_Name.Contains(request.Search) ||
                                     (s.Users.Phone != null && s.Users.Phone.Contains(request.Search)));
        }

        var total = await query.CountAsync(cancellationToken);
        var students = await query.Skip((request.Page - 1) * request.Per_Page).Take(request.Per_Page).ToListAsync(cancellationToken);

        var data = students.Select(s => new
        {
            id = s.Id,
            student_no = s.Student_No,
            first_name = s.Users?.First_Name,
            last_name = s.Users?.Last_Name,
            email = s.Users?.Email,
            phone = s.Users?.Phone,
            guardian_name = s.Guardian_Name,
            guardian_phone = s.Guardian_Phone,
            school_name = s.School_Name,
            enrolled_at = s.Enrolled_At
        });

        return new GetStudentsResult(data, total, request.Page, request.Per_Page);
    }
}

public record CreateStudentCommand(
    Guid Institute_Id,
    string First_Name,
    string Last_Name,
    string? Email,
    string Phone,
    string? Nic_Or_Birth_Cert,
    DateOnly? Date_Of_Birth,
    Gender? Gender,
    string? School_Name,
    string? Home_Address,
    string? City,
    string? District,
    string Guardian_Name,
    string Guardian_Phone,
    string? Guardian_Relation) : IRequest<object>;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, object>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public CreateStudentCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<object> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var db = (DbContext)_context;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Phone = request.Phone,
            Password_Hash = _passwordHasher.HashPassword("Student@123"),
            Role = User_Role.Student,
            First_Name = request.First_Name,
            Last_Name = request.Last_Name,
            Is_Active = true,
            Is_Verified = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        db.Set<User>().Add(user);

        var count = await db.Set<Student>().CountAsync(s => s.Institute_Id == request.Institute_Id, cancellationToken);
        var studentNo = $"SE-{DateTime.UtcNow.Year % 100}{(count + 1):D4}";
        var qrSecret = Guid.NewGuid().ToString("N");

        var student = new Student
        {
            Id = Guid.NewGuid(),
            User_Id = user.Id,
            Institute_Id = request.Institute_Id,
            Student_No = studentNo,
            Nic_Or_Birth_Cert = request.Nic_Or_Birth_Cert ?? string.Empty,
            Date_Of_Birth = request.Date_Of_Birth ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15)),
            Gender = request.Gender ?? Domain.Enums.Gender.Male,
            School_Name = request.School_Name ?? string.Empty,
            Home_Address = request.Home_Address ?? string.Empty,
            City = request.City ?? string.Empty,
            District = request.District ?? string.Empty,
            Guardian_Name = request.Guardian_Name,
            Guardian_Phone = request.Guardian_Phone,
            Guardian_Relation = request.Guardian_Relation ?? string.Empty,
            Qr_Secret = qrSecret,
            Enrolled_At = DateOnly.FromDateTime(DateTime.UtcNow),
            Is_Active = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        db.Set<Student>().Add(student);

        var qrTokenStr = _jwtTokenService.GenerateQrToken(student.Id, qrSecret);
        db.Set<Student_Qr_Token>().Add(new Student_Qr_Token
        {
            Id = Guid.NewGuid(),
            Student_Id = student.Id,
            Token = qrTokenStr,
            Qr_Data = qrTokenStr,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new
        {
            id = student.Id,
            student_no = student.Student_No,
            user_id = user.Id,
            first_name = user.First_Name,
            last_name = user.Last_Name,
            qr_secret = student.Qr_Secret
        };
    }
}
