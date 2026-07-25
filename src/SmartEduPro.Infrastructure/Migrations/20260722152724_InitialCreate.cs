using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace SmartEduPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activity_Logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Actor = table.Column<string>(type: "text", nullable: false),
                    Target_Type = table.Column<string>(type: "text", nullable: false),
                    Target_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Ip_Address = table.Column<string>(type: "text", nullable: true),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Author_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Target_Class_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Target_Role = table.Column<int>(type: "integer", nullable: true),
                    Is_Pinned = table.Column<bool>(type: "boolean", nullable: false),
                    Send_Push = table.Column<bool>(type: "boolean", nullable: false),
                    Send_Sms = table.Column<bool>(type: "boolean", nullable: false),
                    Send_Whatsapp = table.Column<bool>(type: "boolean", nullable: false),
                    Published_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Expires_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Billing_Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Amount_Lkr = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Payment_Method = table.Column<string>(type: "text", nullable: false),
                    Payment_Ref = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BillingPeriod = table.Column<NpgsqlRange<DateTime>>(type: "tstzrange", nullable: false),
                    Invoice_Url = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Class_Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Monthly_Fee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Institute_Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Branch_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Permission = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institute_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Institutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Registration_No = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    Logo_Url = table.Column<string>(type: "text", nullable: false),
                    Address_Line1 = table.Column<string>(type: "text", nullable: false),
                    Address_Line2 = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    District = table.Column<string>(type: "text", nullable: false),
                    Province = table.Column<string>(type: "text", nullable: false),
                    Postal_Code = table.Column<string>(type: "text", nullable: false),
                    Owner_Name = table.Column<string>(type: "text", nullable: false),
                    Owner_Phone = table.Column<string>(type: "text", nullable: false),
                    Owner_Email = table.Column<string>(type: "text", nullable: false),
                    Subscription_Plan = table.Column<int>(type: "integer", nullable: false),
                    Subscription_Status = table.Column<int>(type: "integer", nullable: false),
                    Subscription_Start = table.Column<DateOnly>(type: "date", nullable: false),
                    Subscription_End = table.Column<DateOnly>(type: "date", nullable: false),
                    Trial_End = table.Column<DateOnly>(type: "date", nullable: false),
                    Max_student = table.Column<int>(type: "integer", nullable: false),
                    Max_Teacher = table.Column<int>(type: "integer", nullable: false),
                    Setting = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Live_Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TimeTable_Slot_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Scheduled_Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Scheduled_End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Actual_Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Actual_End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Room_Id = table.Column<string>(type: "text", nullable: false),
                    Room_Token = table.Column<string>(type: "text", nullable: false),
                    Join_Url = table.Column<string>(type: "text", nullable: false),
                    Recording_Url = table.Column<string>(type: "text", nullable: false),
                    Participant_Count = table.Column<int>(type: "integer", nullable: false),
                    Peak_Viewers = table.Column<int>(type: "integer", nullable: false),
                    Chat_Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Recording_Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    WhiteBoard_Data = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Live_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sender_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Is_Read = table.Column<bool>(type: "boolean", nullable: false),
                    Read_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attachment_Url = table.Column<string>(type: "text", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification_Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Title_Template = table.Column<string>(type: "text", nullable: false),
                    Body_Template = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Recipient_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Is_Read = table.Column<bool>(type: "boolean", nullable: false),
                    Read_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Is_Delivered = table.Column<bool>(type: "boolean", nullable: false),
                    Delivered_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivery_Error = table.Column<string>(type: "text", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Otp_Codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    Expires_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otp_Codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Occupation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Whatsapp_No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Alt_Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Preffered_Channel = table.Column<int>(type: "integer", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Student_Class_Enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Enrolled_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Left_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Fee_Package_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Class_Enrollments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscription_Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<int>(type: "integer", nullable: false),
                    Display_Name = table.Column<string>(type: "text", nullable: false),
                    Price_Lkr = table.Column<decimal>(type: "numeric", nullable: false),
                    Max_Student = table.Column<int>(type: "integer", nullable: false),
                    Max_Teacher = table.Column<int>(type: "integer", nullable: false),
                    Max_Branch = table.Column<int>(type: "integer", nullable: false),
                    Feature = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teacher_Class_Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Is_Primary = table.Column<bool>(type: "boolean", nullable: false),
                    From_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    To_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher_Class_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nic = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Date_Of_Birth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Qualification = table.Column<string>(type: "text", nullable: false),
                    Specialization = table.Column<string>(type: "text", nullable: false),
                    Experience_Yrs = table.Column<int>(type: "integer", nullable: false),
                    Photo_Url = table.Column<string>(type: "text", nullable: false),
                    Bank_Name = table.Column<string>(type: "text", nullable: false),
                    Bank_Account = table.Column<string>(type: "text", nullable: false),
                    Salary_Lkr = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Joined_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Password_Hash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    First_Name = table.Column<string>(type: "text", nullable: false),
                    Last_Name = table.Column<string>(type: "text", nullable: false),
                    Avatar_Url = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Verified = table.Column<bool>(type: "boolean", nullable: false),
                    Last_Login_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fcm_Token = table.Column<string>(type: "text", nullable: false),
                    Apns_Token = table.Column<string>(type: "text", nullable: false),
                    Preferred_Lan = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Manager_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Insitute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Study_Level = table.Column<int>(type: "integer", nullable: false),
                    Al_Stream = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InstitutesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Institutes_InstitutesId",
                        column: x => x.InstitutesId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Live_Session_Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Session_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Joined_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Left_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration_Secs = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Live_Session_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Live_Session_Participants_Live_Sessions_Session_Id",
                        column: x => x.Session_Id,
                        principalTable: "Live_Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Live_Session_Participants_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refresh_Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token_Hash = table.Column<string>(type: "text", nullable: false),
                    Device_Info = table.Column<string>(type: "text", nullable: false),
                    Ip_Address = table.Column<IPAddress>(type: "inet", nullable: false),
                    Expire_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refresh_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refresh_Tokens_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nic_Or_Birth_Cert = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date_Of_Birth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    School_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Home_Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Guardian_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Guardian_Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Guardian_Relation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Photo_Url = table.Column<string>(type: "text", nullable: false),
                    Qr_Code_Url = table.Column<string>(type: "text", nullable: false),
                    Qr_Secret = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Enrolled_At = table.Column<DateOnly>(type: "date", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Branch_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Study_Level = table.Column<int>(type: "integer", nullable: false),
                    Al_Stream = table.Column<int>(type: "integer", nullable: false),
                    Academic_year = table.Column<int>(type: "integer", nullable: false),
                    Class_Type = table.Column<int>(type: "integer", nullable: false),
                    Max_Student = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Branches_Branch_Id",
                        column: x => x.Branch_Id,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Classes_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student_Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Parent_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Relation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Is_Primary = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Parents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Parents_Parents_Parent_Id",
                        column: x => x.Parent_Id,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Student_Parents_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student_Qr_Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Qr_Data = table.Column<string>(type: "text", nullable: false),
                    Expires_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Used_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Qr_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Qr_Tokens_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendance_Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Session_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Start_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Is_Completed = table.Column<bool>(type: "boolean", nullable: false),
                    Total_Present = table.Column<int>(type: "integer", nullable: false),
                    Total_Absent = table.Column<int>(type: "integer", nullable: false),
                    Total_Late = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Sessions_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Sessions_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Sessions_Subjects_Subject_Id",
                        column: x => x.Subject_Id,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Sessions_Teachers_Teacher_Id",
                        column: x => x.Teacher_Id,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Exam_Type = table.Column<int>(type: "integer", nullable: false),
                    Exam_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Start_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Duration_Min = table.Column<int>(type: "integer", nullable: false),
                    Total_Mark = table.Column<decimal>(type: "numeric", nullable: false),
                    Pass_Mark = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: false),
                    Is_Published = table.Column<bool>(type: "boolean", nullable: false),
                    Published_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Subjects_Subject_Id",
                        column: x => x.Subject_Id,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Teachers_Teacher_Id",
                        column: x => x.Teacher_Id,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fee_Packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount_Lkr = table.Column<decimal>(type: "numeric", nullable: false),
                    Billing_Cycle = table.Column<string>(type: "text", nullable: false),
                    Due_Day = table.Column<int>(type: "integer", nullable: false),
                    Late_Fee_Lkr = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fee_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fee_Packages_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fee_Packages_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Learning_Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Uploaded_By = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    File_Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    File_Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    File_Size_Mb = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Download_Count = table.Column<int>(type: "integer", nullable: false),
                    Is_Published = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learning_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Learning_Resources_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Learning_Resources_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learning_Resources_Subjects_Subject_Id",
                        column: x => x.Subject_Id,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Learning_Resources_Users_Uploaded_By",
                        column: x => x.Uploaded_By,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recordings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Session_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Duration_Sec = table.Column<int>(type: "integer", nullable: false),
                    File_Url = table.Column<string>(type: "text", nullable: false),
                    Thumbnail_Url = table.Column<string>(type: "text", nullable: false),
                    File_Size_Mb = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    View_Count = table.Column<int>(type: "integer", nullable: false),
                    Is_Published = table.Column<bool>(type: "boolean", nullable: false),
                    Recorded_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recordings_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recordings_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recordings_Live_Sessions_Session_Id",
                        column: x => x.Session_Id,
                        principalTable: "Live_Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recordings_Subjects_Subject_Id",
                        column: x => x.Subject_Id,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recordings_Teachers_Teacher_Id",
                        column: x => x.Teacher_Id,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timetable_Slots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Branch_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Class_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teacher_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Day_Of_Week = table.Column<int>(type: "integer", nullable: false),
                    Start_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: false),
                    Class_Type = table.Column<int>(type: "integer", nullable: false),
                    Effective_From = table.Column<DateOnly>(type: "date", nullable: false),
                    Effective_To = table.Column<DateOnly>(type: "date", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timetable_Slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timetable_Slots_Branches_Branch_Id",
                        column: x => x.Branch_Id,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timetable_Slots_Classes_Class_Id",
                        column: x => x.Class_Id,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timetable_Slots_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timetable_Slots_Subjects_Subject_Id",
                        column: x => x.Subject_Id,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendance_Records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Session_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Check_In_Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Check_Out_Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Marked_Via = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Qr_Scan_Log = table.Column<string>(type: "text", nullable: false),
                    Late_Minutes = table.Column<int>(type: "integer", nullable: false),
                    Parent_Notified = table.Column<bool>(type: "boolean", nullable: false),
                    Notified_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance_Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Records_Attendance_Sessions_Session_Id",
                        column: x => x.Session_Id,
                        principalTable: "Attendance_Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Records_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exam_Results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Exam_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Marks_Obtained = table.Column<decimal>(type: "numeric", nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    Rank_Int_Class = table.Column<int>(type: "integer", nullable: false),
                    Is_Absent = table.Column<bool>(type: "boolean", nullable: false),
                    Teacher_Notes = table.Column<string>(type: "text", nullable: false),
                    Parent_Notified = table.Column<bool>(type: "boolean", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Results_Exams_Exam_Id",
                        column: x => x.Exam_Id,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exam_Results_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student_Fee_Records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fee_Package_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Billing_Month = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount_Due = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Late_Fee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Fee_Status = table.Column<int>(type: "integer", nullable: false),
                    Due_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Reminder_Count = table.Column<int>(type: "integer", nullable: false),
                    Last_Reminder_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Fee_Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Fee_Records_Fee_Packages_Fee_Package_Id",
                        column: x => x.Fee_Package_Id,
                        principalTable: "Fee_Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Student_Fee_Records_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Student_Fee_Records_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timetable_Overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Time_Table_Slot_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Override_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    New_Start_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    New_End_Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    New_Room = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Notify_Student = table.Column<bool>(type: "boolean", nullable: false),
                    Created_By = table.Column<Guid>(type: "uuid", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timetable_Overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timetable_Overrides_Timetable_Slots_Time_Table_Slot_Id",
                        column: x => x.Time_Table_Slot_Id,
                        principalTable: "Timetable_Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timetable_Overrides_Users_Created_By",
                        column: x => x.Created_By,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fee_Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fee_Record_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount_Lkr = table.Column<decimal>(type: "numeric", nullable: false),
                    Payment_Method = table.Column<int>(type: "integer", nullable: false),
                    Payment_Ref = table.Column<string>(type: "text", nullable: false),
                    Payment_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Received_By = table.Column<Guid>(type: "uuid", nullable: false),
                    Receipt_No = table.Column<string>(type: "text", nullable: false),
                    Receipt_Url = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fee_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fee_Payments_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fee_Payments_Student_Fee_Records_Fee_Record_Id",
                        column: x => x.Fee_Record_Id,
                        principalTable: "Student_Fee_Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fee_Payments_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fee_Payments_Users_Received_By",
                        column: x => x.Received_By,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Invoice_No = table.Column<string>(type: "text", nullable: false),
                    Student_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Institute_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fee_Record_Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount_Lkr = table.Column<decimal>(type: "numeric", nullable: false),
                    Issued_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Due_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Pdf_Url = table.Column<string>(type: "text", nullable: false),
                    Sent_Via = table.Column<string>(type: "text", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Institutes_Institute_Id",
                        column: x => x.Institute_Id,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Student_Fee_Records_Fee_Record_Id",
                        column: x => x.Fee_Record_Id,
                        principalTable: "Student_Fee_Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Records_Session_Id",
                table: "Attendance_Records",
                column: "Session_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Records_Student_Id",
                table: "Attendance_Records",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Sessions_Class_Id",
                table: "Attendance_Sessions",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Sessions_Institute_Id",
                table: "Attendance_Sessions",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Sessions_Subject_Id",
                table: "Attendance_Sessions",
                column: "Subject_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Sessions_Teacher_Id",
                table: "Attendance_Sessions",
                column: "Teacher_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Institute_Id",
                table: "Branches",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Branch_Id",
                table: "Classes",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Institute_Id",
                table: "Classes",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Results_Exam_Id",
                table: "Exam_Results",
                column: "Exam_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Results_Student_Id",
                table: "Exam_Results",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Class_Id",
                table: "Exams",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Institute_Id",
                table: "Exams",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Subject_Id",
                table: "Exams",
                column: "Subject_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Teacher_Id",
                table: "Exams",
                column: "Teacher_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Packages_Class_Id",
                table: "Fee_Packages",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Packages_Institute_Id",
                table: "Fee_Packages",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Payments_Fee_Record_Id",
                table: "Fee_Payments",
                column: "Fee_Record_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Payments_Institute_Id",
                table: "Fee_Payments",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Payments_Received_By",
                table: "Fee_Payments",
                column: "Received_By");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Payments_Student_Id",
                table: "Fee_Payments",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Fee_Record_Id",
                table: "Invoices",
                column: "Fee_Record_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Institute_Id",
                table: "Invoices",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Student_Id",
                table: "Invoices",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Resources_Class_Id",
                table: "Learning_Resources",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Resources_Institute_Id",
                table: "Learning_Resources",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Resources_Subject_Id",
                table: "Learning_Resources",
                column: "Subject_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Resources_Uploaded_By",
                table: "Learning_Resources",
                column: "Uploaded_By");

            migrationBuilder.CreateIndex(
                name: "IX_Live_Session_Participants_Session_Id",
                table: "Live_Session_Participants",
                column: "Session_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Live_Session_Participants_User_Id",
                table: "Live_Session_Participants",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_Class_Id",
                table: "Recordings",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_Institute_Id",
                table: "Recordings",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_Session_Id",
                table: "Recordings",
                column: "Session_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_Subject_Id",
                table: "Recordings",
                column: "Subject_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_Teacher_Id",
                table: "Recordings",
                column: "Teacher_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Refresh_Tokens_User_Id",
                table: "Refresh_Tokens",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Fee_Records_Fee_Package_Id",
                table: "Student_Fee_Records",
                column: "Fee_Package_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Fee_Records_Institute_Id",
                table: "Student_Fee_Records",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Fee_Records_Student_Id",
                table: "Student_Fee_Records",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Parents_Parent_Id",
                table: "Student_Parents",
                column: "Parent_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Parents_Student_Id",
                table: "Student_Parents",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Qr_Tokens_Student_Id",
                table: "Student_Qr_Tokens",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Students_User_Id",
                table: "Students",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_InstitutesId",
                table: "Subjects",
                column: "InstitutesId");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Overrides_Created_By",
                table: "Timetable_Overrides",
                column: "Created_By");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Overrides_Time_Table_Slot_Id",
                table: "Timetable_Overrides",
                column: "Time_Table_Slot_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Slots_Branch_Id",
                table: "Timetable_Slots",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Slots_Class_Id",
                table: "Timetable_Slots",
                column: "Class_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Slots_Institute_Id",
                table: "Timetable_Slots",
                column: "Institute_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Timetable_Slots_Subject_Id",
                table: "Timetable_Slots",
                column: "Subject_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activity_Logs");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "Attendance_Records");

            migrationBuilder.DropTable(
                name: "Billing_Transactions");

            migrationBuilder.DropTable(
                name: "Class_Subjects");

            migrationBuilder.DropTable(
                name: "Exam_Results");

            migrationBuilder.DropTable(
                name: "Fee_Payments");

            migrationBuilder.DropTable(
                name: "Institute_Admins");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Learning_Resources");

            migrationBuilder.DropTable(
                name: "Live_Session_Participants");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notification_Templates");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Otp_Codes");

            migrationBuilder.DropTable(
                name: "Recordings");

            migrationBuilder.DropTable(
                name: "Refresh_Tokens");

            migrationBuilder.DropTable(
                name: "Student_Class_Enrollments");

            migrationBuilder.DropTable(
                name: "Student_Parents");

            migrationBuilder.DropTable(
                name: "Student_Qr_Tokens");

            migrationBuilder.DropTable(
                name: "Subscription_Plans");

            migrationBuilder.DropTable(
                name: "Teacher_Class_Assignments");

            migrationBuilder.DropTable(
                name: "Timetable_Overrides");

            migrationBuilder.DropTable(
                name: "Attendance_Sessions");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Student_Fee_Records");

            migrationBuilder.DropTable(
                name: "Live_Sessions");

            migrationBuilder.DropTable(
                name: "Parents");

            migrationBuilder.DropTable(
                name: "Timetable_Slots");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Fee_Packages");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Institutes");
        }
    }
}
