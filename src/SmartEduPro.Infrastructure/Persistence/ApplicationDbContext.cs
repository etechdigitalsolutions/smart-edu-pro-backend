using Microsoft.EntityFrameworkCore;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Domain.Entities;

namespace SmartEduPro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Institute> Institutes => Set<Institute>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Institute_Admin> Institute_Admins => Set<Institute_Admin>();
    public DbSet<Subscription_Plan> Subscription_Plans => Set<Subscription_Plan>();
    public DbSet<Billing_Transaction> Billing_Transactions => Set<Billing_Transaction>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Class_Subject> Class_Subjects => Set<Class_Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Student_Class_Enrollment> Student_Class_Enrollments => Set<Student_Class_Enrollment>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Teacher_Class_Assignment> Teacher_Class_Assignments => Set<Teacher_Class_Assignment>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Student_Parent> Student_Parents => Set<Student_Parent>();
    public DbSet<Attendance_Session> Attendance_Sessions => Set<Attendance_Session>();
    public DbSet<Attendance_Record> Attendance_Records => Set<Attendance_Record>();
    public DbSet<Student_Qr_Token> Student_Qr_Tokens => Set<Student_Qr_Token>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Exam_Result> Exam_Results => Set<Exam_Result>();
    public DbSet<Timetable_Slot> Timetable_Slots => Set<Timetable_Slot>();
    public DbSet<Timetable_Override> Timetable_Overrides => Set<Timetable_Override>();
    public DbSet<Fee_Package> Fee_Packages => Set<Fee_Package>();
    public DbSet<Student_Fee_Record> Student_Fee_Records => Set<Student_Fee_Record>();
    public DbSet<Fee_Payment> Fee_Payments => Set<Fee_Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Live_Session> Live_Sessions => Set<Live_Session>();
    public DbSet<Live_Session_Participant> Live_Session_Participants => Set<Live_Session_Participant>();
    public DbSet<Recording> Recordings => Set<Recording>();
    public DbSet<Learning_Resource> Learning_Resources => Set<Learning_Resource>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Notification_Template> Notification_Templates => Set<Notification_Template>();
    public DbSet<Activity_Log> Activity_Logs => Set<Activity_Log>();
    public DbSet<Otp_Code> Otp_Codes => Set<Otp_Code>();
    public DbSet<Refresh_Token> Refresh_Tokens => Set<Refresh_Token>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
