using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/billing")]
public class BillingController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public BillingController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription([FromQuery] Guid institute_id)
    {
        var db = (DbContext)_context;
        var inst = await db.Set<Institute>().FindAsync(institute_id);
        if (inst == null) return ApiError("INSTITUTE_NOT_FOUND", "Institute not found", StatusCodes.Status404NotFound);

        return ApiOk(new
        {
            plan = inst.Subscription_Plan.ToString(),
            status = inst.Subscription_Status.ToString(),
            start_date = inst.Subscription_Start,
            end_date = inst.Subscription_End,
            trial_end = inst.Trial_End,
            max_students = inst.Max_student,
            max_teachers = inst.Max_Teacher
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetBillingHistory([FromQuery] Guid institute_id)
    {
        var db = (DbContext)_context;
        var history = await db.Set<Billing_Transaction>()
            .Where(b => b.Institute_Id == institute_id)
            .OrderByDescending(b => b.Created_At)
            .ToListAsync();

        return ApiOk(history);
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> UpgradePlan([FromBody] UpgradePlanReq req)
    {
        var db = (DbContext)_context;
        var inst = await db.Set<Institute>().FindAsync(req.Institute_Id);
        if (inst == null) return ApiError("INSTITUTE_NOT_FOUND", "Institute not found", StatusCodes.Status404NotFound);

        if (Enum.TryParse<Subscription_Plans>(req.Plan, true, out var plan))
        {
            inst.Subscription_Plan = plan;
            inst.Subscription_Status = Subscription_Status.Active;
            inst.Subscription_End = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12));
            inst.Updated_At = DateTime.UtcNow;

            db.Set<Billing_Transaction>().Add(new Billing_Transaction
            {
                Id = Guid.NewGuid(),
                Institute_Id = inst.Id,
                Plan = plan,
                Amount_Lkr = req.Amount_Lkr,
                Currency = "LKR",
                Payment_Method = req.Payment_Method ?? "ONLINE",
                Payment_Ref = req.Payment_Ref ?? string.Empty,
                Status = Payment_Status.PAID,
                BillingPeriod = new NpgsqlRange<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddMonths(12)),
                Created_At = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        return ApiOk(inst, "SaaS Subscription upgraded successfully");
    }
}

public record UpgradePlanReq(
    Guid Institute_Id,
    string Plan,
    decimal Amount_Lkr,
    string? Payment_Method,
    string? Payment_Ref);
