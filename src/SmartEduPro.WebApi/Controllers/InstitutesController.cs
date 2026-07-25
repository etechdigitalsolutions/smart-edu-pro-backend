using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/institutes")]
public class InstitutesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public InstitutesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInstitute(Guid id)
    {
        var db = (DbContext)_context;
        var inst = await db.Set<Institute>().FindAsync(id);
        if (inst == null || inst.Deleted_At != null)
            return ApiError("INSTITUTE_NOT_FOUND", "Institute not found", StatusCodes.Status404NotFound);

        return ApiOk(inst);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateInstitute(Guid id, [FromBody] UpdateInstituteReq req)
    {
        var db = (DbContext)_context;
        var inst = await db.Set<Institute>().FindAsync(id);
        if (inst == null || inst.Deleted_At != null)
            return ApiError("INSTITUTE_NOT_FOUND", "Institute not found", StatusCodes.Status404NotFound);

        if (!string.IsNullOrWhiteSpace(req.Name)) inst.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Phone)) inst.Phone = req.Phone;
        if (!string.IsNullOrWhiteSpace(req.Email)) inst.Email = req.Email;
        if (!string.IsNullOrWhiteSpace(req.Website)) inst.Website = req.Website;
        if (!string.IsNullOrWhiteSpace(req.Address_Line1)) inst.Address_Line1 = req.Address_Line1;
        if (!string.IsNullOrWhiteSpace(req.City)) inst.City = req.City;

        inst.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(inst, "Institute profile updated");
    }

    [HttpGet("{id}/branches")]
    public async Task<IActionResult> GetBranches(Guid id)
    {
        var db = (DbContext)_context;
        var branches = await db.Set<Branch>()
            .Where(b => b.Institute_Id == id && b.Is_Active)
            .ToListAsync();

        return ApiOk(branches);
    }

    [HttpPost("{id}/branches")]
    public async Task<IActionResult> AddBranch(Guid id, [FromBody] CreateBranchReq req)
    {
        var db = (DbContext)_context;
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Institute_Id = id,
            Name = req.Name,
            Address = req.Address,
            City = req.City,
            Phone = req.Phone,
            Is_Active = true,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };

        db.Set<Branch>().Add(branch);
        await _context.SaveChangesAsync();

        return ApiCreated(branch, "Branch created successfully");
    }
}

public record UpdateInstituteReq(
    string? Name,
    string? Phone,
    string? Email,
    string? Website,
    string? Address_Line1,
    string? City);

public record CreateBranchReq(
    string Name,
    string? Address,
    string? City,
    string? Phone);
