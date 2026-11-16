using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shikhsa.Services
{
   

    public class LeaveService 
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public LeaveService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<LeavePageViewModel> GetLeaveDashboardAsync(int userId, UserType userType, string academicYear)
        {
            var vm = new LeavePageViewModel
            {
                CurrentUserId = userId,
                CurrentUserType = userType,
                CurrentAcademicYear = academicYear
            };

            // Rule 7: Fetch Logged-in User's Own History for Current Academic Year
            vm.MyLeaveHistory = await _context.LeaveApplications
                .Where(l => l.ApplicantId == userId
                         && l.ApplicantType == userType
                         && l.AcademicYear == academicYear)
                .OrderByDescending(l => l.AppliedDate)
                .ToListAsync();

            // Rule 4, 6, 9: Fetch Pending Approvals for Admin or Class Teacher
            var today = DateTime.Today;
            List<LeaveApplication> rawLeaves = new();

            if (userType == UserType.Admin)
            {
                // Admin sees all leaves except their own
                rawLeaves = await _context.LeaveApplications
                    .Where(l => l.AcademicYear == academicYear && !(l.ApplicantId == userId && l.ApplicantType == UserType.Admin))
                    .ToListAsync();
            }
            else if (userType == UserType.Staff)
            {
                var assignedClassIds = await _context.ClassTeachers
                    .Where(ct => ct.StaffId == userId)
                    .Select(ct => ct.ClassId)
                    .ToListAsync();

                if (assignedClassIds.Any())
                {
                    vm.IsClassTeacher = true;
                    // Class teacher sees only student leaves of their assigned class
                    rawLeaves = await _context.LeaveApplications
                        .Where(l => l.ApplicantType == UserType.Student
                                 && l.ClassId.HasValue
                                 && assignedClassIds.Contains(l.ClassId.Value)
                                 && l.AcademicYear == academicYear)
                        .ToListAsync();
                }
            }

            // Rule 9: Pending or Today's date applications appear at the TOP
            vm.PendingApprovals = rawLeaves
                .OrderByDescending(l => l.Status == LeaveStatus.Pending || l.AppliedDate.Date == today)
                .ThenByDescending(l => l.AppliedDate)
                .Select(l => new LeaveApplicationApprovalDTO
                {
                    LeaveId = l.LeaveId,
                    ApplicantId = l.ApplicantId,
                    ApplicantType = l.ApplicantType,
                    ApplicantName = GetApplicantName(l.ApplicantId, l.ApplicantType),
                    LeaveSubject = l.LeaveSubject,
                    LeaveTypeName = l.LeaveTypeName,
                    IsPaidLeave = l.IsPaidLeave,
                    NoOfDays = l.NoOfDays,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    ApplicationBody = l.ApplicationBody,
                    Status = l.Status,
                    AppliedDate = l.AppliedDate,
                    IsTodayOrPending = (l.Status == LeaveStatus.Pending || l.AppliedDate.Date == today)
                }).ToList();

            return vm;
        }

        public async Task<(bool Success, string Message)> ApplyLeaveAsync(LeavePageViewModel model, int? studentClassId)
        {
            var leave = new LeaveApplication
            {
                ApplicantId = model.CurrentUserId,
                ApplicantType = model.CurrentUserType,
                NoOfDays = model.NoOfDays,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                ApplicationBody = model.ApplicationBody,
                AcademicYear = model.CurrentAcademicYear,
                AppliedDate = DateTime.Now,
                Status = LeaveStatus.Pending
            };

            if (model.CurrentUserType == UserType.Student)
            {
                leave.LeaveSubject = model.LeaveSubject;
                leave.ClassId = studentClassId;
            }
            else
            {
                leave.LeaveTypeName = model.LeaveTypeName;
                leave.IsPaidLeave = model.IsPaidLeave;
            }

            _context.LeaveApplications.Add(leave);
            await _context.SaveChangesAsync();
            return (true, "Leave application submitted successfully.");
        }

        public async Task<(bool Success, string Message)> ProcessLeaveActionAsync(
            int leaveId, int approverId, UserType approverType, bool isApproved, string? remarks)
        {
            var leave = await _context.LeaveApplications.FindAsync(leaveId);
            if (leave == null) return (false, "Leave application not found.");

            // Rule 5: Nobody can approve their own leave
            if (leave.ApplicantId == approverId && leave.ApplicantType == approverType)
                return (false, "You cannot approve or reject your own leave application.");

            // Rule 3: Students cannot approve
            if (approverType == UserType.Student)
                return (false, "Unauthorized action.");

            // Rule 6: Staff leave can ONLY be approved by Admin
            if (leave.ApplicantType == UserType.Staff && approverType != UserType.Admin)
                return (false, "Only Admin can approve Staff leave applications.");

            leave.Status = isApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
            leave.ActionTakenById = approverId;
            leave.ActionDate = DateTime.Now;
            leave.ApproverRemarks = remarks;

            // Rule 8: Mark Attendance Table if Approved
            if (isApproved)
            {
                await MarkAttendanceAsLeaveAsync(leave);
            }

            await _context.SaveChangesAsync();

            // Rule 10: Send Email Notification
            string userEmail = GetApplicantEmail(leave.ApplicantId, leave.ApplicantType);
            if (!string.IsNullOrEmpty(userEmail))
            {
                string statusText = isApproved ? "Approved" : "Rejected";
                string subject = $"Leave Application {statusText}";
                string body = $"Dear User,<br/><br/>Your leave application from <b>{leave.FromDate:dd-MMM-yyyy}</b> to <b>{leave.ToDate:dd-MMM-yyyy}</b> ({leave.NoOfDays} days) has been <b>{statusText}</b>.<br/>Remarks: {remarks ?? "N/A"}";

                await _emailService.SendEmailAsync(userEmail, subject, body);
            }

            return (true, $"Leave has been {(isApproved ? "Approved" : "Rejected")}.");
        }

        private async Task MarkAttendanceAsLeaveAsync(LeaveApplication leave)
        {
            for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
            {
                var existingRecord = await _context.Attendances.FirstOrDefaultAsync(a =>
                    a.UserId == leave.ApplicantId &&
                    a.UserType == leave.ApplicantType &&
                    a.AttendanceDate.Date == date);

                if (existingRecord != null)
                {
                    existingRecord.Status = "Leave";
                    existingRecord.Remarks = $"Approved Leave (#{leave.LeaveId})";
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        UserId = leave.ApplicantId,
                        UserType = leave.ApplicantType,
                        AttendanceDate = date,
                        Status = "Leave",
                        Remarks = $"Approved Leave (#{leave.LeaveId})"
                    });
                }
            }
        }

        private string GetApplicantName(int id, UserType type)
        {
            return type == UserType.Student
                ? _context.Students.Where(s => s.StudentId == id).Select(s => s.FullName).FirstOrDefault() ?? "Student"
                : _context.Staffs.Where(s => s.StaffId == id).Select(s => s.FullName).FirstOrDefault() ?? "Staff";
        }

        private string GetApplicantEmail(int id, UserType type)
        {
            return type == UserType.Student
                ? _context.Students.Where(s => s.StudentId == id).Select(s => s.Email).FirstOrDefault() ?? ""
                : _context.Staffs.Where(s => s.StaffId == id).Select(s => s.Email).FirstOrDefault() ?? "";
        }
    }
}