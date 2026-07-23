//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

//namespace Shikhsa.Data
//{
//    public class ApplicationDbContext : IdentityDbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }
//    }
//}
// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Models.Notification;
using Shikhsa.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace Shikhsa.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options) { _httpContextAccessor = httpContextAccessor; }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<Language> Languages { get; set; }

        public DbSet<Translation> Translations { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions{ get; set; }
        public DbSet<DataList> DataLists { get; set; }
        public DbSet<Batches> Batches { get; set; }
        public DbSet<SchoolMaster> SchoolMasters { get; set; }
        public DbSet<DataListItem> DataListItems { get; set; }
        public DbSet<Tbl_StudentsRegistrations> Tbl_StudentsRegistrations { get; set; }
        public DbSet<Tbl_Parents> Tbl_Parents { get; set; }
        public DbSet<Tbl_StudentDocument> Tbl_StudentDocument { get; set; }
        public DbSet<Tbl_PreviousSchoolRecord> Tbl_PreviousSchoolRecord { get; set; }
        public DbSet<Tbl_Students> Tbl_Students { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<SubjectMasters> SubjectMasters { get; set; }
        public DbSet<ClassBatchSubjectHeader> ClassBatchSubjectHeaders { get; set; }
        public DbSet<ClassBatchSubjectDetail> ClassBatchSubjectDetails { get; set; }
        public DbSet<ExamCategory> ExamCategories { get; set; }
        public DbSet<StaffMaster> StaffMasters { get; set; }

        public DbSet<StaffAcademic> StaffAcademics { get; set; }

        public DbSet<StaffExperience> StaffExperiences { get; set; }

        public DbSet<StaffDocument> StaffDocuments { get; set; }

        public DbSet<StaffEmergencyContact> StaffEmergencyContacts { get; set; }

        public DbSet<StaffLeaveSetting> StaffLeaveSettings { get; set; }
        public DbSet<ScholasticExam> scholasticExams { get; set; }
        public DbSet<ClassTeacherSubjectAssignment> ClassTeacherSubjectAssignments { get; set; }
        public DbSet<ClassTeacher> ClassTeachers { get; set; }
        public DbSet<GradingCriteria> GradingCriteria { get; set; }
        public DbSet<AttendanceType> AttendanceTypes { get; set; }

        public DbSet<StaffAttendance> StaffAttendances { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }

        public DbSet<NotificationCategory> NotificationCategories { get; set; }

        public DbSet<NotificationPlaceholder> NotificationPlaceholders { get; set; }

        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<NotificationTemplateCategory> NotificationTemplateCategories { get; set; }
        public DbSet<FeeHeading> FeeHeadings { get; set; }

        public DbSet<FeeFrequency> FeeFrequencies { get; set; }
        public DbSet<TuitionFeePlan> TuitionFeePlans  { get; set; }
        public DbSet<TransportFeePlan> TransportFeePlans { get; set; }
        public DbSet<HostelFeePlan> HostelFeePlans { get; set; }
        public DbSet<CoScholasticArea> CoScholasticAreas { get; set; }

        public DbSet<CoScholastic> CoScholastics { get; set; }
        public DbSet<Tbl_ExamObtainedMarks> ExamObtainedMarks { get; set; }
        public DbSet<StudentExamSummary> StudentExamSummaries { get; set; }
        public DbSet<CoScholasticGrade> CoScholasticGrades { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                System.Diagnostics.Debug.WriteLine(entity.ClrType.FullName);
            }

            base.OnModelCreating(builder);

            // Ignore MigrationOperation entity - it shouldn't be persisted
            builder.Ignore<MigrationOperation>();

            builder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ... rest of your configuration
     

        

            //builder.Entity<Menu>()
            //    .HasOne(m => m.Parent)
            //    .WithMany(m => m.Children)
            //    .HasForeignKey(m => m.ParentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany()
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoleMenu>()
                .HasIndex(rm => new { rm.RoleId, rm.MenuId })
                .IsUnique();
            builder.Entity<DataListItem>()
                .HasOne(x => x.DataList)
                .WithMany(x => x.DataListItems)
                .HasForeignKey(x => x.DataListId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<StaffMaster>()
.HasIndex(x => x.AadhaarNumber)
.IsUnique();
            builder.Entity<StaffMaster>().ToTable("StaffMaster");
            builder.Entity<StaffAcademic>().ToTable("StaffAcademic");
            builder.Entity<StaffExperience>().ToTable("StaffExperience");
            builder.Entity<StaffDocument>().ToTable("StaffDocument");
            builder.Entity<StaffEmergencyContact>().ToTable("StaffEmergencyContact");
            builder.Entity<StaffLeaveSetting>().ToTable("StaffLeaveSetting");
            builder.Entity<StaffMaster>().HasIndex(x => x.Email).IsUnique();

            builder.Entity<StaffMaster>().HasIndex(x => x.MobileNo).IsUnique();

            builder.Entity<StaffMaster>().HasIndex(x => x.StaffCode).IsUnique();

            builder.Entity<StaffMaster>().HasIndex(x => x.AadhaarNumber).IsUnique();
            builder.Entity<StaffMaster>().HasOne<StaffMaster>().WithMany().HasForeignKey(x => x.ReportingStaffId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<StaffMaster>().HasOne(s => s.User) .WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.SetNull);
            foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
            builder.Entity<GradingCriteria>().HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId);

            builder.Entity<GradingCriteria>().HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId);

            builder.Entity<GradingCriteria>().HasOne(x => x.Term).WithMany().HasForeignKey(x => x.TermId);
            builder.Entity<StaffAttendance>().HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StaffAttendance>().HasOne(x => x.AttendanceType).WithMany().HasForeignKey(x => x.AttendanceTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<StaffAttendance>().HasIndex(x => new{ x.StaffId,x.AttendanceDate}).IsUnique();
            builder.Entity<ClassTeacherSubjectAssignment>().HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassTeacherSubjectAssignment>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassTeacherSubjectAssignment>().HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NotificationCategory>().HasIndex(x => x.CategoryCode).IsUnique();

            builder.Entity<NotificationTemplate>().HasIndex(x => x.TemplateCode).IsUnique();

            builder.Entity<NotificationPlaceholder>().HasIndex(x => x.PlaceholderCode).IsUnique();

            builder.Entity<NotificationTemplateCategory>().HasKey(x => new { x.NotificationTemplateId, x.NotificationCategoryId });

            //builder.Entity<NotificationTemplateCategory>().HasOne(x => x.NotificationTemplate).WithMany(x => x.NotificationTemplateCategories).HasForeignKey(x => x.NotificationTemplateId);

            //builder.Entity<NotificationTemplateCategory>().HasOne(x => x.NotificationCategory).WithMany(x => x.NotificationTemplateCategories).HasForeignKey(x => x.NotificationCategoryId);

            builder.Entity<NotificationPlaceholder>().HasOne(x => x.NotificationCategory).WithMany(x => x.NotificationPlaceholders).HasForeignKey(x => x.NotificationCategoryId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<NotificationTemplateCategory>().HasOne(x => x.NotificationTemplate).WithMany(x => x.NotificationTemplateCategories).HasForeignKey(x => x.NotificationTemplateId).OnDelete(DeleteBehavior.NoAction);

            builder.Entity<NotificationTemplateCategory>().HasOne(x => x.NotificationCategory).WithMany(x => x.NotificationTemplateCategories).HasForeignKey(x => x.NotificationCategoryId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<FeeFrequency>().HasData(new FeeFrequency{    FrequencyId = 1,    Value = "ONETIME",    Text = "One Time",    DisplayOrder = 1,    IsActive = true, AddedDate = new DateTime(2025, 1, 1) }, new FeeFrequency{    FrequencyId = 2,    Value = "MONTHLY",    Text = "Monthly",    DisplayOrder = 2,    IsActive = true, AddedDate = new DateTime(2025, 1, 1) }, new FeeFrequency{    FrequencyId = 3,    Value = "QUARTERLY",    Text = "Quarterly",    DisplayOrder = 3,    IsActive = true, AddedDate = new DateTime(2025, 1, 1) },new FeeFrequency{    FrequencyId = 4,    Value = "HALFYEARLY",    Text = "Half Yearly",    DisplayOrder = 4,    IsActive = true, AddedDate = new DateTime(2025, 1, 1) },new FeeFrequency{    FrequencyId = 5,    Value = "YEARLY",    Text = "Yearly",    DisplayOrder = 5,    IsActive = true, AddedDate = new DateTime(2025, 1, 1) } );
            builder.Entity<MenuPermissionItem>().HasOne(x => x.SubMenu).WithMany().HasForeignKey(x => x.SubMenuId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<TuitionFeePlan>().HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TuitionFeePlan>().HasOne(x => x.FeeHeading).WithMany().HasForeignKey(x => x.FeeHeadingId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TransportFeePlan>().HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TransportFeePlan>().HasOne(x => x.FeeHeading).WithMany().HasForeignKey(x => x.FeeHeadingId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<HostelFeePlan>().HasOne(x => x.FeeHeading).WithMany().HasForeignKey(x => x.FeeHeadingId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CoScholasticArea>().HasOne<CoScholastic>().WithMany().HasForeignKey(x => x.CoScholasticId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CoScholasticArea>().HasOne<DataListItem>().WithMany().HasForeignKey(x => x.ClassId).HasPrincipalKey(x => x.DataListItemId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>() .HasOne(x => x.Class) .WithMany() .HasForeignKey(x => x.ClassId) .HasPrincipalKey(x => x.DataListItemId) .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>().HasOne(x => x.ExamTypes).WithMany().HasForeignKey(x => x.ExamType).HasPrincipalKey(x => x.DataListItemId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>().HasOne<Batches>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>().HasOne<ExamCategory>().WithMany().HasForeignKey(x => x.ExamCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ScholasticExam>(entity => {entity.HasKey(x => x.Id); entity.HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).HasPrincipalKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId).HasPrincipalKey(x => x.DataListItemId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.ExamTypes).WithMany().HasForeignKey(x => x.ExamType).HasPrincipalKey(x => x.DataListItemId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.ExamCategory).WithMany().HasForeignKey(x => x.ExamCategoryId).OnDelete(DeleteBehavior.Restrict);
            });
           builder.Entity<StudentExamSummary>().HasIndex(x => new{x.BatchId,x.ClassId,x.SectionId,x.ExamCategoryId,x.StudentId}).IsUnique();
            builder.Entity<StudentExamSummary>(entity =>
            {
                entity.HasOne(s => s.Class).WithMany().HasForeignKey(s => s.ClassId).OnDelete(DeleteBehavior.NoAction);entity.HasOne(s => s.Section).WithMany().HasForeignKey(s => s.SectionId).OnDelete(DeleteBehavior.NoAction);entity.HasOne(s => s.Student).WithMany().HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.NoAction);entity.HasOne(s => s.Batch).WithMany().HasForeignKey(s => s.BatchId).OnDelete(DeleteBehavior.NoAction);entity.HasOne(s => s.ExamCategory).WithMany().HasForeignKey(s => s.ExamCategoryId).OnDelete(DeleteBehavior.NoAction);
            });
            builder.Entity<CoScholasticArea>().HasOne(x => x.CoScholastic).WithMany().HasForeignKey(x => x.CoScholasticId).HasPrincipalKey(x => x.CoScholasticId).OnDelete(DeleteBehavior.Restrict);
        }
        public override int SaveChanges()
        {
            var currentUser = _httpContextAccessor.HttpContext?.Session.GetCurrentUser();

            string userName = currentUser?.UserName
                              ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                              ?? "";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.AddedDate = DateTime.Now;
                    entry.Entity.AddedBy = userName;
                    entry.Entity.IsActive = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedDate = DateTime.Now;
                    entry.Entity.UpdatedBy = userName;

                    entry.Property(x => x.AddedDate).IsModified = false;
                    entry.Property(x => x.AddedBy).IsModified = false;
                }
            }

            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentUser = _httpContextAccessor.HttpContext?.Session.GetCurrentUser();

            string userName = currentUser?.UserName
                              ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                              ?? "";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.AddedDate = DateTime.Now;
                    entry.Entity.AddedBy = userName;
                    entry.Entity.IsActive = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedDate = DateTime.Now;
                    entry.Entity.UpdatedBy = userName;

                    entry.Property(x => x.AddedDate).IsModified = false;
                    entry.Property(x => x.AddedBy).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}