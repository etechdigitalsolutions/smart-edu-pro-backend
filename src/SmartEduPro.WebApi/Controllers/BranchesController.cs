using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/branches")]
public class BranchesController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public BranchesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPatch("{branchId}")]
    public async Task<IActionResult> UpdateBranch(Guid branchId, [FromBody] UpdateBranchReq req)
    {
        var db = (DbContext)_context;
        var branch = await db.Set<Branch>().FindAsync(branchId);
        if (branch == null)
            return ApiError("BRANCH_NOT_FOUND", "Branch not found", StatusCodes.Status404NotFound);

        if (!string.IsNullOrWhiteSpace(req.Name)) branch.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Address)) branch.Address = req.Address;
        if (!string.IsNullOrWhiteSpace(req.City)) branch.City = req.City;
        if (!string.IsNullOrWhiteSpace(req.Phone)) branch.Phone = req.Phone;

        branch.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiOk(branch, "Branch updated successfully");
    }

    [HttpDelete("{branchId}")]
    public async Task<IActionResult> DeleteBranch(Guid branchId)
    {
        var db = (DbContext)_context;
        var branch = await db.Set<Branch>().FindAsync(branchId);
        if (branch == null)
            return ApiError("BRANCH_NOT_FOUND", "Branch not found", StatusCodes.Status404NotFound);

        branch.Is_Active = false;
        branch.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiDeleted("Branch deactivated successfully");
    }
}

public record UpdateBranchReq(string? Name, string? Address, string? City, string? Phone);
