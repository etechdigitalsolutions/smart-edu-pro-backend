using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/finance")]
public class FinanceController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public FinanceController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetFeePackages([FromQuery] Guid? institute_id)
    {
        var db = (DbContext)_context;
        var query = db.Set<Fee_Package>().AsNoTracking().Where(p => p.Is_Active);
        if (institute_id.HasValue) query = query.Where(p => p.Institute_Id == institute_id.Value);

        var packages = await query.ToListAsync();
        return ApiOk(packages);
    }

    [HttpPost("packages")]
    public async Task<IActionResult> CreateFeePackage([FromBody] CreatePackageReq req)
    {
        var db = (DbContext)_context;
        var pkg = new Fee_Package
        {
            Id = Guid.NewGuid(),
            Institute_Id = req.Institute_Id,
            Name = req.Name,
            Class_Id = req.Class_Id ?? Guid.Empty,
            Amount_Lkr = req.Amount_Lkr,
            Billing_Cycle = req.Billing_Cycle ?? "MONTHLY",
            Due_Day = req.Due_Day ?? 5,
            Late_Fee_Lkr = req.Late_Fee_Lkr ?? 0,
            Description = req.Description ?? string.Empty,
            Is_Active = true,
            Created_At = DateTime.UtcNow
        };

        db.Set<Fee_Package>().Add(pkg);
        await _context.SaveChangesAsync();

        return ApiCreated(pkg, "Fee package created");
    }

    [HttpPost("records/generate")]
    public async Task<IActionResult> GenerateFeeRecords([FromBody] GenerateFeeRecordsReq req)
    {
        var db = (DbContext)_context;
        var students = await db.Set<Student_Class_Enrollment>()
            .Where(e => e.Class_Id == req.Class_Id && e.Is_Active)
            .Select(e => e.Student_Id)
            .ToListAsync();

        int generatedCount = 0;
        foreach (var studentId in students)
        {
            var exists = await db.Set<Student_Fee_Record>().AnyAsync(r => r.Student_Id == studentId && r.Fee_Package_Id == req.Fee_Package_Id && r.Billing_Month == req.Billing_Month);
            if (!exists)
            {
                db.Set<Student_Fee_Record>().Add(new Student_Fee_Record
                {
                    Id = Guid.NewGuid(),
                    Student_Id = studentId,
                    Institute_Id = req.Institute_Id,
                    Fee_Package_Id = req.Fee_Package_Id,
                    Billing_Month = req.Billing_Month,
                    Amount_Due = req.Amount_Due,
                    Late_Fee = 0,
                    Discount = 0,
                    Balance = req.Amount_Due,
                    Fee_Status = Fee_Status.Pending,
                    Due_Date = req.Billing_Month.AddDays(5),
                    Created_At = DateTime.UtcNow,
                    Updated_At = DateTime.UtcNow
                });
                generatedCount++;
            }
        }

        await _context.SaveChangesAsync();
        return ApiOk(new { generated_count = generatedCount }, "Fee records generated");
    }

    [HttpGet("records")]
    public async Task<IActionResult> GetFeeRecords([FromQuery] Guid? student_id, [FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Student_Fee_Record>().AsNoTracking();
        if (student_id.HasValue) query = query.Where(r => r.Student_Id == student_id.Value);

        var total = await query.CountAsync();
        var records = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(records, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpPost("payments")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentReq req)
    {
        var db = (DbContext)_context;
        var feeRecord = await db.Set<Student_Fee_Record>().FindAsync(req.Fee_Record_Id);
        if (feeRecord == null) return ApiError("FEE_RECORD_NOT_FOUND", "Fee record not found", StatusCodes.Status404NotFound);

        var receiptNo = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpper();

        var payment = new Fee_Payment
        {
            Id = Guid.NewGuid(),
            Fee_Record_Id = req.Fee_Record_Id,
            Student_Id = feeRecord.Student_Id,
            Institute_Id = feeRecord.Institute_Id,
            Amount_Lkr = req.Amount_Lkr,
            Payment_Method = Enum.TryParse<Payment_Method>(req.Payment_Method, true, out var pm) ? pm : Payment_Method.Cash,
            Payment_Ref = req.Payment_Ref ?? string.Empty,
            Payment_Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Receipt_No = receiptNo,
            Created_At = DateTime.UtcNow
        };
        db.Set<Fee_Payment>().Add(payment);

        feeRecord.Balance -= req.Amount_Lkr;
        if (feeRecord.Balance <= 0)
        {
            feeRecord.Fee_Status = Fee_Status.Paid;
        }
        else
        {
            feeRecord.Fee_Status = Fee_Status.Partial;
        }
        feeRecord.Updated_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiCreated(payment, "Payment recorded and receipt generated");
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] int page = 1, [FromQuery] int per_page = 20)
    {
        var db = (DbContext)_context;
        var query = db.Set<Fee_Payment>().AsNoTracking().OrderByDescending(p => p.Created_At);
        var total = await query.CountAsync();
        var payments = await query.Skip((page - 1) * per_page).Take(per_page).ToListAsync();

        return ApiOk(payments, meta: new { page, per_page, total, total_pages = (int)Math.Ceiling(total / (double)per_page) });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetFinanceSummary([FromQuery] Guid institute_id)
    {
        var db = (DbContext)_context;
        var totalCollected = await db.Set<Fee_Payment>().Where(p => p.Institute_Id == institute_id).SumAsync(p => (decimal?)p.Amount_Lkr) ?? 0;
        var totalPending = await db.Set<Student_Fee_Record>().Where(r => r.Institute_Id == institute_id && r.Fee_Status == Fee_Status.Pending).SumAsync(r => (decimal?)r.Amount_Due) ?? 0;
        var totalOverdue = await db.Set<Student_Fee_Record>().Where(r => r.Institute_Id == institute_id && r.Fee_Status == Fee_Status.Overdue).SumAsync(r => (decimal?)r.Amount_Due) ?? 0;

        return ApiOk(new
        {
            total_collected_lkr = totalCollected,
            total_pending_lkr = totalPending,
            total_overdue_lkr = totalOverdue
        });
    }
}

public record CreatePackageReq(
    Guid Institute_Id,
    string Name,
    Guid? Class_Id,
    decimal Amount_Lkr,
    string? Billing_Cycle,
    int? Due_Day,
    decimal? Late_Fee_Lkr,
    string? Description);

public record GenerateFeeRecordsReq(
    Guid Institute_Id,
    Guid Class_Id,
    Guid Fee_Package_Id,
    DateOnly Billing_Month,
    decimal Amount_Due);

public record RecordPaymentReq(
    Guid Fee_Record_Id,
    decimal Amount_Lkr,
    string Payment_Method,
    string? Payment_Ref);
