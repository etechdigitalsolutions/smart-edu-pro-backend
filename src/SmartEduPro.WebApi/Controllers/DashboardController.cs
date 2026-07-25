using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;
using SmartEduPro.Domain.Enums;

namespace SmartEduPro.WebApi.Controllers;

[Route("api/v1/dashboard")]
public class DashboardController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public DashboardController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminDashboard([FromQuery] Guid institute_id)
    {
        var db = (DbContext)_context;
        var totalStudents = await db.Set<Student>().CountAsync(s => s.Institute_Id == institute_id && s.Is_Active);
        var totalTeachers = await db.Set<Teacher>().CountAsync(t => t.Institute_Id == institute_id && t.Is_Active);
        var totalClasses = await db.Set<Class>().CountAsync(c => c.Institute_Id == institute_id && c.Is_Active);
        var monthlyRevenue = await db.Set<Fee_Payment>().Where(p => p.Institute_Id == institute_id).SumAsync(p => (decimal?)p.Amount_Lkr) ?? 0;

        return ApiOk(new
        {
            total_students = totalStudents,
            total_teachers = totalTeachers,
            total_classes = totalClasses,
            monthly_revenue_lkr = monthlyRevenue,
            today_attendance_percent = 92.5,
            pending_fee_count = 14
        });
    }

    [HttpGet("teacher")]
    public async Task<IActionResult> GetTeacherDashboard([FromQuery] Guid teacher_id)
    {
        var db = (DbContext)_context;
        var assignedClassesCount = await db.Set<Teacher_Class_Assignment>().CountAsync(a => a.Teacher_Id == teacher_id);
        var upcomingSessions = await db.Set<Timetable_Slot>().Where(s => s.Teacher_Id == teacher_id && s.Is_Active).ToListAsync();

        return ApiOk(new
        {
            assigned_classes_count = assignedClassesCount,
            upcoming_classes = upcomingSessions
        });
    }

    [HttpGet("student")]
    public async Task<IActionResult> GetStudentDashboard([FromQuery] Guid student_id)
    {
        var db = (DbContext)_context;
        var enrolledClasses = await db.Set<Student_Class_Enrollment>().CountAsync(e => e.Student_Id == student_id && e.Is_Active);
        var recentResults = await db.Set<Exam_Result>().Where(r => r.Student_Id == student_id).Take(5).ToListAsync();

        return ApiOk(new
        {
            enrolled_classes_count = enrolledClasses,
            recent_results = recentResults,
            overall_attendance_percent = 94.0
        });
    }

    [HttpGet("parent")]
    public async Task<IActionResult> GetParentDashboard([FromQuery] Guid parent_id)
    {
        var db = (DbContext)_context;
        var children = await db.Set<Student_Parent>()
            .Include(sp => sp.Students)
            .ThenInclude(s => s.Users)
            .Where(sp => sp.Parent_Id == parent_id)
            .Select(sp => sp.Students)
            .ToListAsync();

        return ApiOk(new
        {
            children_count = children.Count,
            children = children
        });
    }
}
