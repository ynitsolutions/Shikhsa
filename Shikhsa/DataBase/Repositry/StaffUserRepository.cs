using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Shikhsa.DataBase.Repositry
{
    public class StaffUserRepository
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<ApplicationRole> _roleManager;
        public readonly NotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LookupService _lookup;
        public StaffUserRepository
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            NotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            LookupService lookup
        )
        {
            _context = context;

            _userManager = userManager;

            _roleManager = roleManager;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _lookup = lookup;
        }
        public async Task<StaffUserVM> GetPageData()
        {
            StaffUserVM model = new StaffUserVM();

            model.StaffList = await _context.StaffMasters

                .Where(x => x.UserId == null)

                .OrderBy(x => x.FirstName).ThenBy(x => x.MiddleName).ThenBy(x => x.LastName)

                .Select(x => new SelectListItem
                {
                    Value = x.StaffId.ToString(),
                    Text = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }).ToListAsync();


            model.RoleList = await _roleManager.Roles

                .OrderBy(x => x.Name)

                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.Name
                }).ToListAsync();

            model.UserList = await GetUserList();

            return model;
        }
        private async Task<List<StaffUserListVM>> GetUserList()
        {
            var users =

                from s in _context.StaffMasters

                join u in _context.Users

                on s.UserId equals u.Id

                join ur in _context.UserRoles

                on u.Id equals ur.UserId

                join r in _context.Roles

                on ur.RoleId equals r.Id

                orderby s.FirstName, s.MiddleName, s.LastName   

                select new StaffUserListVM
                {
                    UserId = u.Id,

                    StaffId = (int)s.StaffId,

                    StaffName = s.FirstName+s.MiddleName+s.LastName,

                    UserName = u.UserName,

                    Email = u.Email,

                    Password = u.NormalPassword,

                    RoleName = r.Name
                };

            return await users.ToListAsync();
        }
        //     public async Task<ResponseModel> CreateUser(long staffId, string roleId)
        //     {
        //         ResponseModel response = new();

        //         var staff = await (
        //    from s in _context.StaffMasters
        //    join d in _context.DataListItems
        //        on s.DepartmentId equals d.DataListItemId into dept
        //    from d in dept.DefaultIfEmpty()   // LEFT JOIN
        //    where s.StaffId == staffId
        //    select new
        //    {
        //        Staff = s,
        //        DepartmentName = d != null ? d.DataListItemText : ""
        //    }
        //).FirstOrDefaultAsync();


        //         if (staff == null)
        //         {
        //             response.Status = 0;

        //             response.Message = "Staff not found.";

        //             return response;
        //         }

        //         if (!string.IsNullOrEmpty(staff.Staff.UserId))
        //         {
        //             response.Status = 0;

        //             response.Message = "Login already exists.";

        //             return response;
        //         }

        //         if (await _userManager.FindByEmailAsync(staff.Staff.Email) != null)
        //         {
        //             response.Status = 0;

        //             response.Message = "Email already exists.";

        //             return response;
        //         }

        //         string password = staff.Staff.DOB.ToString("ddMMyyyy");

        //         ApplicationUser user = new()
        //         {
        //             UserName = staff.Staff.Email,

        //             Email = staff.Staff.Email,

        //             FullName = staff.Staff.FirstName + staff.Staff.MiddleName + staff.Staff.LastName,

        //             Department = staff.DepartmentName,

        //             CreatedAt = DateTime.Now,

        //             IsActive = true,

        //             NormalPassword = password
        //         };

        //         var result = await _userManager.CreateAsync(user, password);

        //         if (!result.Succeeded)
        //         {
        //             response.Status = 0;

        //             response.Message = string.Join(",", result.Errors.Select(x => x.Description));

        //             return response;
        //         }

        //         var role = await _roleManager.FindByIdAsync(roleId);

        //         await _userManager.AddToRoleAsync(user, role.Name);

        //         staff.Staff.UserId = user.Id;

        //         _context.Update(staff);

        //         await _context.SaveChangesAsync();

        //         response.Status = 1;

        //         response.Message = "User Created Successfully.";

        //         return response;
        //     }

        public async Task<ResponseModel> CreateUser(long staffId, string roleId)
        {
            ResponseModel response = new();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var staff = await (
                    from s in _context.StaffMasters
                    join d in _context.DataListItems
                        on s.DepartmentId equals d.DataListItemId into dept
                    from d in dept.DefaultIfEmpty()
                    where s.StaffId == staffId
                    select new
                    {
                        Staff = s,
                        DepartmentName = d != null ? d.DataListItemText : ""
                    }).FirstOrDefaultAsync();

                if (staff == null)
                {
                    response.Status = 0;
                    response.Message = "Staff not found.";
                    return response;
                }

                if (!string.IsNullOrEmpty(staff.Staff.UserId))
                {
                    response.Status = 0;
                    response.Message = "Login already exists.";
                    return response;
                }

                if (await _userManager.FindByEmailAsync(staff.Staff.Email) != null)
                {
                    response.Status = 0;
                    response.Message = "Email already exists.";
                    return response;
                }

                string password = staff.Staff.DOB.ToString("ddMMyyyy");

                ApplicationUser user = new()
                {
                    UserName = staff.Staff.Email,
                    Email = staff.Staff.Email,
                    FullName = $"{staff.Staff.FirstName} {staff.Staff.MiddleName} {staff.Staff.LastName}",
                    Department = staff.DepartmentName,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    NormalPassword = password
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    response.Status = 0;
                    response.Message = string.Join(",", result.Errors.Select(x => x.Description));
                    return response;
                }

                var role = await _roleManager.FindByIdAsync(roleId);

                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }

                // Update Staff
                staff.Staff.UserId = user.Id;

                // Update() ki zarurat nahi, entity tracked hai.
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                response.Status = 1;
                response.Message = "User Created Successfully.";
                await 
                    _lookup.PopulateAsync(staff.Staff);
                await _notificationService.SendAsync(
                     "Staff_Login_Credentials",
                     staff.Staff.Email,
                     staff.Staff.StaffId,
                     staff.Staff,
                     user
                    );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ResponseModel> UpdateRole(string userId, string roleId)
        {
            ResponseModel response = new();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                response.Status = 0;
                response.Message = "User not found.";
                return response;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                response.Status = 0;
                response.Message = "Role not found.";
                return response;
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                response.Status = 0;
                response.Message = string.Join(",", result.Errors.Select(x => x.Description));
                return response;
            }

            response.Status = 1;
            response.Message = "Role updated successfully.";

            return response;
        }
        //public async Task<ResponseModel> ChangePassword(ChangePasswordVM model, bool isAdmin)
        //{
        //    ResponseModel response = new();

        //    var user = await _userManager.FindByIdAsync(model.UserId);

        //    if (user == null)
        //    {
        //        response.Status = 0;
        //        response.Message = "User not found.";
        //        return response;
        //    }

        //    IdentityResult result;

        //    if (isAdmin)
        //    {
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //        result = await _userManager.ResetPasswordAsync(
        //                        user,
        //                        token,
        //                        model.NewPassword);
        //    }
        //    else
        //    {
        //        result = await _userManager.ChangePasswordAsync(
        //                        user,
        //                        model.OldPassword,
        //                        model.NewPassword);
        //    }

        //    if (!result.Succeeded)
        //    {
        //        response.Status = 0;
        //        response.Message = string.Join(",", result.Errors.Select(x => x.Description));
        //        return response;
        //    }

        //    user.NormalPassword = model.NewPassword;

        //    await _userManager.UpdateAsync(user);

        //    response.Status = 1;
        //    response.Message = "Password changed successfully.";

        //    return response;
        //}
        public async Task<ResponseModel> ChangePassword(ChangePasswordVM model)
        {
            ResponseModel response = new();

            // 1. Basic User Verification
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                response.Status = 0;
                response.Message = "User not found.";
                return response;
            }

            // 2. Fetch the Current Logged-in Requestor from HttpContext
            // This removes the security flaw by verifying credentials server-side.
            var currentUserId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
            bool isTargetingSelf = (currentUserId == model.UserId);
            bool isRequestorAdmin = _httpContextAccessor.HttpContext.User.IsInRole("Admin");

            IdentityResult result;

            // Case A: A user is updating their own password (Regardless of their specific role)
            if (isTargetingSelf)
            {
                if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    response.Status = 0;
                    response.Message = "Current password is required to update your own account.";
                    return response;
                }

                result = await _userManager.ChangePasswordAsync(
                                user,
                                model.OldPassword,
                                model.NewPassword);
            }
            // Case B: An Admin is resetting a student/teacher/staff account password
            else if (isRequestorAdmin)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(
                                user,
                                token,
                                model.NewPassword);
            }
            // Case C: Unauthorized attempt (e.g., a teacher trying to change a student's password directly)
            else
            {
                response.Status = 0;
                response.Message = "Access denied. You do not have permission to modify this account.";
                return response;
            }

            // 3. Handle Identity Pipeline Errors
            if (!result.Succeeded)
            {
                response.Status = 0;
                response.Message = string.Join(" ", result.Errors.Select(x => x.Description));
                return response;
            }

            // 4. Mirror values (Optional tracking column setup)
            user.NormalPassword = model.NewPassword;
            await _userManager.UpdateAsync(user);

            response.Status = 1;
            response.Message = "Password changed successfully.";

            return response;
        }

        public async Task<StaffUserVM?> GetUserForEdit(string userId)
        {
            var data = await (
                from s in _context.StaffMasters
                join u in _context.Users
                    on s.UserId equals u.Id
                join ur in _context.UserRoles
                    on u.Id equals ur.UserId
                where u.Id == userId
                select new StaffUserVM
                {
                    UserId = u.Id,
                    StaffId = s.StaffId,
                    RoleId = ur.RoleId
                }).FirstOrDefaultAsync();

            if (data == null)
                return null;

            // Reload dropdowns
            data.StaffList = await _context.StaffMasters
                .OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem
                {
                    Value = x.StaffId.ToString(),
                    Text = x.FirstName + " " + x.LastName
                }).ToListAsync();

            data.RoleList = await _roleManager.Roles
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.Name
                }).ToListAsync();

            return data;
        }
        public async Task<List<SelectListItem>> GetStaffDropdown()
        {
            return await _context.StaffMasters

                    .Where(x => x.IsActive==true)

                    .OrderBy(x => x.FirstName)

                    .Select(x => new SelectListItem
                    {
                        Value = x.StaffId.ToString(),
                        Text = x.FirstName + " " + x.LastName
                    })
                    .ToListAsync();
        }
        //public async Task<List<StaffUserListVM>> GetStaffUserListAsync()
        //{
        //    var data = await (
        //        from s in _context.StaffMasters
        //        join u in _context.Users
        //            on s.UserId equals u.Id
        //        select new StaffUserListVM
        //        {
        //            UserId = u.Id,
        //            StaffId = s.StaffId,
        //            StaffName = s.FirstName+" "+s.MiddleName+" "+s.LastName,
        //            UserName = u.UserName,
        //            Email = u.Email,
        //            Password = u.NormalPassword,
        //          IsActive=u.IsActive,

        //        })
        //        .OrderBy(x => x.StaffName)
        //        .ToListAsync();

        //    return data;
        //}
        public async Task<List<StaffUserListVM>> GetStaffUserListAsync()
        {
            var data = await (
                from s in _context.StaffMasters
                join u in _context.Users
                    on s.UserId equals u.Id
                join ur in _context.UserRoles
                    on u.Id equals ur.UserId into urGroup
                from ur in urGroup.DefaultIfEmpty()
                join r in _context.Roles
                    on ur.RoleId equals r.Id into rGroup
                from r in rGroup.DefaultIfEmpty()
                select new StaffUserListVM
                {
                    UserId = u.Id,
                    StaffId = s.StaffId,
                    StaffName = s.FirstName + " " + s.MiddleName + " " + s.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    Password = u.NormalPassword,
                    IsActive = u.IsActive,
                    RoleName = r != null ? r.Name : null,
                })
                .OrderBy(x => x.StaffName)
                .ToListAsync();
            return data;
        }
        public async Task<ResponseModel> UpdateUserRole(string userId, string oldRoleId, string newRoleId)
        {
            ResponseModel response = new();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // User find karo
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    response.Status = 0;
                    response.Message = "User not found.";
                    return response;
                }

                // Old role find karo
                var oldRole = await _roleManager.FindByIdAsync(oldRoleId);
                if (oldRole == null)
                {
                    response.Status = 0;
                    response.Message = "Old role not found.";
                    return response;
                }

                // New role find karo
                var newRole = await _roleManager.FindByIdAsync(newRoleId);
                if (newRole == null)
                {
                    response.Status = 0;
                    response.Message = "New role not found.";
                    return response;
                }

                // Sirf old role remove karo
                var removeResult = await _userManager.RemoveFromRoleAsync(user, oldRole.Name);
                if (!removeResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    response.Status = 0;
                    response.Message = string.Join(", ", removeResult.Errors.Select(x => x.Description));
                    return response;
                }

                // New role assign karo
                var addResult = await _userManager.AddToRoleAsync(user, newRole.Name);
                if (!addResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    response.Status = 0;
                    response.Message = string.Join(", ", addResult.Errors.Select(x => x.Description));
                    return response;
                }

                await transaction.CommitAsync();
                response.Status = 1;
                response.Message = "Role Updated Successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Status = 0;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
