using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/exams")]
public class ExamsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public ExamsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetExams([FromQuery] Guid? class_id, [FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Exam>().AsNoTracking();
        if (class_id.HasValue) query = query.Where(e => e.Class_Id == class_id.Value);

        var total = await query.CountAsync();
        var exams = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(exams, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamReq req)
    {
        var db = (DbContext)_context;
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Class_Id = req.Class_Id,
            Subject_Id = req.Subject_Id,
            Teacher_Id = req.Teacher_Id,
            Title = req.Title,
            Exam_Type = Enum.TryParse<Exam_Type>(req.Exam_Type, true, out var et) ? et : Exam_Type.Test,
            Exam_Date = req.Exam_Date,
            Total_Mark = req.Total_Marks,
            Pass_Mark = req.Pass_Marks ?? (req.Total_Marks * 0.4m),
            Is_Published = false,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Exam>().Add(exam);
        await _context.SaveChangesAsync();

        return ApiCreated(exam, "Exam scheduled successfully");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExam(Guid id)
    {
        var db = (DbContext)_context;
        var exam = await db.Set<Exam>().FindAsync(id);
        if (exam == null) return ApiError("EXAM_NOT_FOUND", "Exam not found", StatusCodes.Status404NotFound);
        return ApiOk(exam);
    }

    [HttpPost("{id}/results/bulk")]
    public async Task<IActionResult> RecordBulkResults(Guid id, [FromBody] BulkResultsReq req)
    {
        var db = (DbContext)_context;
        var exam = await db.Set<Exam>().FindAsync(id);
        if (exam == null) return ApiError("EXAM_NOT_FOUND", "Exam not found", StatusCodes.Status404NotFound);

        foreach (var r in req.Results)
        {
            var existing = await db.Set<Exam_Result>()
                .FirstOrDefaultAsync(x => x.Exam_Id == id && x.Student_Id == r.Student_Id);

            var grade = CalculateGrade(r.Marks_Obtained, exam.Total_Mark);

            if (existing == null)
            {
                db.Set<Exam_Result>().Add(new Exam_Result
                {
                    Id = Guid.NewGuid(),
                    Exam_Id = id,
                    Student_Id = r.Student_Id,
                    Marks_Obtained = r.Marks_Obtained ?? 0,
                    Grade = grade,
                    Is_Absent = r.Is_Absent ?? false,
                    Teacher_Notes = r.Teacher_Notes ?? string.Empty,
                    Created_At = DateTime.UtcNow,
                    Updated_At = DateTime.UtcNow
                });
            }
            else
            {
                existing.Marks_Obtained = r.Marks_Obtained ?? 0;
                existing.Grade = grade;
                existing.Is_Absent = r.Is_Absent ?? false;
                existing.Teacher_Notes = r.Teacher_Notes ?? string.Empty;
                existing.Updated_At = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return ApiOk(null, "Results saved successfully");
    }

    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResults(Guid id)
    {
        var db = (DbContext)_context;
        var results = await db.Set<Exam_Result>()
            .Include(r => r.Students)
            .Where(r => r.Exam_Id == id)
            .ToListAsync();

        return ApiOk(results);
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishResults(Guid id)
    {
        var db = (DbContext)_context;
        var exam = await db.Set<Exam>().FindAsync(id);
        if (exam == null) return ApiError("EXAM_NOT_FOUND", "Exam not found", StatusCodes.Status404NotFound);

        exam.Is_Published = true;
        exam.Published_At = DateTime.UtcNow;
        exam.Updated_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiOk(exam, "Exam results published to parents & students");
    }

    private static Sl_Grade CalculateGrade(decimal? marks, decimal total)
    {
        if (!marks.HasValue) return Sl_Grade.F;
        var pct = (marks.Value / total) * 100;
        return pct switch
        {
            >= 85 => Sl_Grade.A_Plus,
            >= 75 => Sl_Grade.A,
            >= 65 => Sl_Grade.B,
            >= 55 => Sl_Grade.C,
            >= 40 => Sl_Grade.S,
            _ => Sl_Grade.F
        };
    }
}

public record CreateExamReq(
    Guid Institute_Id,
    Guid Class_Id,
    Guid Subject_Id,
    Guid Teacher_Id,
    string Title,
    string? Exam_Type,
    DateOnly Exam_Date,
    decimal Total_Marks,
    decimal? Pass_Marks);

public record BulkResultsReq(List<StudentResultItem> Results);
public record StudentResultItem(Guid Student_Id, decimal? Marks_Obtained, bool? Is_Absent, string? Teacher_Notes);
