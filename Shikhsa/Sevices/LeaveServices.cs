using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        public LeaveService(ApplicationDbContext context, EmailService emailService, NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<LeavePageViewModel> GetLeaveDashboardAsync(int userId, string roleName)
        {
            var currentBatch = await _context.Batches
                .FirstOrDefaultAsync(b => b.IsCurrentYear == true);

            int currentBatchId = currentBatch?.BatchId ?? 0;
            string currentBatchName = currentBatch?.AcademicYear ?? "Current Session";

            var vm = new LeavePageViewModel
            {
                CurrentUserId = userId,
                CurrentRoleName = roleName,
                CurrentBatchId = currentBatchId,
                CurrentBatchName = currentBatchName
            };

            // Rule 7: User's own history for current academic session
            vm.MyLeaveHistory = await _context.LeaveApplications
                .Where(l => l.ApplicantId == userId
                         && l.ApplicantType.ToLower() == roleName.ToLower()
                         && l.BatchId == currentBatchId)
                .OrderByDescending(l => l.AppliedDate)
                .ToListAsync();
            vm.LeaveTypeDropdown = await _context.AttendanceTypes.Where(a => a.IsLeave == true && a.IsActive == true).OrderBy(a => a.DisplayOrder).Select(a => new SelectListItem
            {
                Value = a.Code, // Saves Code (e.g., "CL", "SL")
                Text = a.Name   // Displays Name (e.g., "Casual Leave")
            }).ToListAsync();
            var today = DateTime.Today;
            List<LeaveApplication> rawLeaves = new();

            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin sees all leaves in current session except their own
                rawLeaves = await _context.LeaveApplications
                    .Where(l => l.BatchId == currentBatchId && !(l.ApplicantId == userId && l.ApplicantType.ToLower() == roleName.ToLower()))
                    .ToListAsync();
            }
            else // Staff / Teacher
            {
                var assignedClassIds = await _context.ClassTeachers
                    .Where(ct => ct.StaffId == userId)
                    .Select(ct => ct.ClassId)
                    .ToListAsync();

                if (assignedClassIds.Any())
                {
                    vm.IsClassTeacher = true;
                    // Class teacher sees only student leaves of their assigned classes
                    rawLeaves = await _context.LeaveApplications
                        .Where(l => l.ApplicantType.ToLower() == "student"
                                 && l.ClassId.HasValue
                                 && assignedClassIds.Contains(l.ClassId.Value)
                                 && l.BatchId == currentBatchId)
                        .ToListAsync();
                }
            }

            // Rule 9: Pending or Today's leaves appear at the top
            vm.PendingApprovals = rawLeaves
    .OrderByDescending(l => l.Status == LeaveStatus.Pending || l.AppliedDate.Date == today)
    .ThenByDescending(l => l.AppliedDate)
    .ToList();

            foreach (var l in vm.PendingApprovals)
                l.ApplicantName = GetApplicantName(l.ApplicantId, l.ApplicantType);

            return vm;
        }

        public async Task<(bool Success, string Message)> ApplyLeaveAsync(LeavePageViewModel model)
        {
            var currentBatch = await _context.Batches
                .FirstOrDefaultAsync(b => b.IsCurrentYear == true);

            if (currentBatch == null)
                return (false, "Active academic session not found in Batches table.");

            var leave = new LeaveApplication
            {
                ApplicantId = model.CurrentUserId,
                ApplicantType = model.CurrentRoleName,
                NoOfDays = model.NoOfDays,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                ApplicationBody = model.ApplicationBody,
                BatchId = currentBatch.BatchId,
                AppliedDate = DateTime.Now,
                Status = LeaveStatus.Pending
            };

            if (model.CurrentRoleName.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                leave.LeaveSubject = model.LeaveSubject;
                leave.ClassId = await _context.Tbl_Students
                    .Where(s => s.StudentId == model.CurrentUserId)
                    .Select(s => (int?)s.AdmitClassId)
                    .FirstOrDefaultAsync();
                leave.LeaveTypeCode = model.LeaveTypeCode;
                leave.IsPaidLeave = model.IsPaidLeave;
            }
            else
            {
                leave.LeaveTypeCode = model.LeaveTypeCode;
                leave.LeaveTypeName = model.LeaveTypeName;
                leave.IsPaidLeave = model.IsPaidLeave;
            }

            _context.LeaveApplications.Add(leave);
            await _context.SaveChangesAsync();
            return (true, "Leave application submitted successfully.");
        }

        public async Task<(bool Success, string Message)> ProcessLeaveActionAsync(
            int leaveId, int approverId, string approverRole, bool isApproved, string? remarks)
        {
            var leave = await _context.LeaveApplications.FindAsync(leaveId);
            if (leave == null) return (false, "Leave application not found.");

            // Rule 5: Nobody can approve their own leave
            if (leave.ApplicantId == approverId && leave.ApplicantType.Equals(approverRole, StringComparison.OrdinalIgnoreCase))
                return (false, "You cannot approve or reject your own leave application.");

            // Rule 3: Students cannot approve
            if (approverRole.Equals("Student", StringComparison.OrdinalIgnoreCase))
                return (false, "Unauthorized action.");

            // Rule 6: Staff leave can ONLY be approved by Admin
            bool isAdmin = approverRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                           approverRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

            if (leave.ApplicantType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && !isAdmin)
                return (false, "Only Admin can approve Staff leave applications.");

            leave.Status = isApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
            leave.ActionTakenById = approverId;
            leave.ActionDate = DateTime.Now;
            leave.ApproverRemarks = remarks;

            // Rule 8: Mark Attendance Table if Approved
            if (isApproved)
            {
                string approverEmail = await _context.StaffMasters
                    .Where(s => s.StaffId == approverId)
                    .Select(s => s.Email)
                    .FirstOrDefaultAsync() ?? approverId.ToString();

                // Fix CS7036: Pass approverEmail as the 2nd argument
                await MarkAttendanceAsLeaveAsync(leave, approverEmail);
            }

            await _context.SaveChangesAsync();

            // Rule 10: Send Email Notification
            string userEmail = GetApplicantEmail(leave.ApplicantId, leave.ApplicantType);
            //if (!string.IsNullOrEmpty(userEmail))
            //{
            //    string statusText = isApproved ? "Approved" : "Rejected";
            //    string subject = $"Leave Application {statusText}";
            //    string body = $"Dear User,<br/><br/>Your leave application from <b>{leave.FromDate:dd-MMM-yyyy}</b> to <b>{leave.ToDate:dd-MMM-yyyy}</b> ({leave.NoOfDays} days) has been <b>{statusText}</b>.<br/>Remarks: {remarks ?? "N/A"}";

            //    //await _emailService.SendEmailAsync(userEmail, subject, body);
            //}

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                object? applicant = null;

                // Get actual applicant entity from database
                if (leave.ApplicantType.Equals(
                    "Staff",
                    StringComparison.OrdinalIgnoreCase))
                {
                    applicant = await _context.StaffMasters
                        .FirstOrDefaultAsync(s =>
                            s.StaffId == leave.ApplicantId);
                }
                else if (leave.ApplicantType.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    // Replace 'TeacherMasters' and 'TeacherId' with your actual DbSet and property names
                    applicant = await _context.StaffMasters
                        .FirstOrDefaultAsync(t => t.StaffId == leave.ApplicantId);
                }
                else if (leave.ApplicantType.Equals(
                    "Student",
                    StringComparison.OrdinalIgnoreCase))
                {
                    applicant = await _context.Tbl_Students
                        .FirstOrDefaultAsync(s =>
                            s.StudentId == leave.ApplicantId);
                }
                else if (leave.ApplicantType.Equals(
              "Students",
              StringComparison.OrdinalIgnoreCase))
                {
                    applicant = await _context.Tbl_Students
                        .FirstOrDefaultAsync(s =>
                            s.StudentId == leave.ApplicantId);
                }
                // Get approver entity
                var actionByUser = await _context.StaffMasters
                    .FirstOrDefaultAsync(s =>
                        s.StaffId == approverId);

                if (applicant != null)
                {
                    var response = await _notificationService.SendAsync(
                        "LEAVE_STATUS",
                        userEmail,
                        leave.LeaveId,
                        leave,
                        applicant,
                        actionByUser
                    );
                }
            }


            return (true, $"Leave has been {(isApproved ? "Approved" : "Rejected")}.");
        }

        //private async Task MarkAttendanceAsLeaveAsync(LeaveApplication leave)
        //{
        //    // Find matching AttendanceTypeId from AttendanceTypes table
        //    AttendanceType? leaveAttendanceType = null;

        //    if (!string.IsNullOrEmpty(leave.LeaveTypeCode))
        //    {
        //        // For Staff: Match the exact Code selected in the dropdown
        //        leaveAttendanceType = await _context.AttendanceTypes
        //            .FirstOrDefaultAsync(a => a.Code == leave.LeaveTypeCode && a.IsActive == true);
        //    }

        //    // Fallback (or for Students): Pick the first active row where IsLeave == true
        //    leaveAttendanceType ??= await _context.AttendanceTypes
        //        .Where(a => a.IsLeave == true && a.IsActive == true)
        //        .OrderBy(a => a.DisplayOrder)
        //        .FirstOrDefaultAsync();

        //    if (leaveAttendanceType == null)
        //        throw new Exception("No active Leave Type found in AttendanceTypes table (IsLeave = 1).");

        //    for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
        //    {
        //        var existingRecord = await _context.StaffAttendances.FirstOrDefaultAsync(a =>
        //            a.StaffId == leave.ApplicantId &&
        //            a.AttendanceDate.Date == date);

        //        if (existingRecord != null)
        //        {
        //            existingRecord.AttendanceTypeId = leaveAttendanceType.AttendanceTypeId;
        //            existingRecord.Remarks = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})";
        //        }
        //        else
        //        {
        //            _context.StaffAttendances.Add(new StaffAttendance
        //            {
        //                StaffId = leave.ApplicantId,
        //                AttendanceDate = date,
        //                AttendanceTypeId = leaveAttendanceType.AttendanceTypeId, // Saves AttendanceTypeId
        //                Remarks = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})"
        //            });
        //        }
        //    }
        //}

        private async Task MarkAttendanceAsLeaveAsync(LeaveApplication leave, string actionByUser)
        {
            // 1. Find matching AttendanceTypeId from AttendanceTypes table
            AttendanceType? leaveAttendanceType = null;

            if (!string.IsNullOrEmpty(leave.LeaveTypeCode))
            {
                leaveAttendanceType = await _context.AttendanceTypes
                    .FirstOrDefaultAsync(a => a.Code == leave.LeaveTypeCode && a.IsActive == true);
            }

            // Fallback (or for Students): Pick the first active row where IsLeave == true
            leaveAttendanceType ??= await _context.AttendanceTypes
                .Where(a => a.IsLeave == true && a.IsActive == true)
                .OrderBy(a => a.DisplayOrder)
                .FirstOrDefaultAsync();

            if (leaveAttendanceType == null)
                throw new Exception("No active Leave Type found in AttendanceTypes table (IsLeave = 1).");

            bool isStudent = leave.ApplicantType.Equals("Student", StringComparison.OrdinalIgnoreCase);

            if (isStudent)
            {
                // Fetch Student's ClassId & SectionId from Students table (or StudentEnrollments)
                var studentInfo = await _context.Tbl_Students
                    .Where(s => s.StudentId == leave.ApplicantId)
                    .Select(s => new { s.AdmitClassId, s.AdmitSectionId })
                    .FirstOrDefaultAsync();

                if (studentInfo == null)
                    throw new Exception("Student record not found for attendance marking.");

                for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
                {

                    var existingStudentAtt = await _context.StudentAttendances.FirstOrDefaultAsync(a =>
                        a.StudentId == leave.ApplicantId &&
                        a.BatchId == leave.BatchId &&
                        a.AttendanceDate == DateOnly.FromDateTime(date));

                    if (existingStudentAtt != null)
                    {
                        existingStudentAtt.AttendanceTypeId = leaveAttendanceType.AttendanceTypeId;
                        existingStudentAtt.Remark = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})";
                        existingStudentAtt.UpdatedDate = DateTime.Now;
                        existingStudentAtt.UpdatedBy = actionByUser;
                        existingStudentAtt.IsActive = true;
                    }
                    else
                    {
                        _context.StudentAttendances.Add(new StudentAttendance
                        {
                            BatchId = leave.BatchId,
                            ClassId = studentInfo.AdmitClassId ?? 0,
                            SectionId = studentInfo.AdmitSectionId ?? 0,
                            StudentId = leave.ApplicantId,
                            AttendanceDate = DateOnly.FromDateTime(date),
                            AttendanceTypeId = leaveAttendanceType.AttendanceTypeId,
                            Remark = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})",
                            IsFreeze = false,
                            AddedDate = DateTime.Now,
                            AddedBy = actionByUser,
                            IsActive = true
                        });
                    }
                }
            }
            else
            {
                // Staff / Admin Attendance
                for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
                {
                    
                    var existingStaffAtt = await _context.StaffAttendances.FirstOrDefaultAsync(a =>
                        a.StaffId == leave.ApplicantId &&
                        a.AttendanceDate == DateOnly.FromDateTime(date));

                    if (existingStaffAtt != null)
                    {
                        existingStaffAtt.AttendanceTypeId = leaveAttendanceType.AttendanceTypeId;
                        existingStaffAtt.Remarks = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})";
                        existingStaffAtt.UpdatedDate = DateTime.Now;
                        existingStaffAtt.UpdatedBy = actionByUser;
                        existingStaffAtt.IsActive = true;
                    }
                    else
                    {
                        _context.StaffAttendances.Add(new StaffAttendance
                        {
                            StaffId = leave.ApplicantId,
                            AttendanceDate = DateOnly.FromDateTime(date),
                            AttendanceTypeId = leaveAttendanceType.AttendanceTypeId,
                            Remarks = $"Approved Leave (#{leave.LeaveId} - {leaveAttendanceType.Code})",
                            AddedDate = DateTime.Now,
                            AddedBy = actionByUser,
                            IsActive = true
                        });
                    }
                }
            }
        }

        private string GetApplicantName(int id, string type)
        {
            return type.Equals("Student", StringComparison.OrdinalIgnoreCase)
                ? _context.Tbl_Students.Where(s => s.StudentId == id).Select(s => s.FirstName + " "+s.MiddleName+" " + s.LastName).FirstOrDefault() ?? "Student"
                : _context.StaffMasters.Where(s => s.StaffId == id).Select(s => s.FullName).FirstOrDefault() ?? "Staff";
        }

        private string GetApplicantEmail(int id, string type)
        {
            return type.Equals("Student", StringComparison.OrdinalIgnoreCase)
                ? _context.Tbl_Students.Where(s => s.StudentId == id).Select(s => s.Email).FirstOrDefault() ?? ""
                : _context.StaffMasters.Where(s => s.StaffId == id).Select(s => s.Email).FirstOrDefault() ?? "";
        }
    }
}