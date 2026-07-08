using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Text.Json;
namespace Shikhsa.Controllers
{
    public class BaseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PermissionService _permissionService;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _email;
        public BaseController(UserManager<ApplicationUser> userManager, PermissionService permissionService, ApplicationDbContext context, EmailService email)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _context = context;
           _email= email;
        }

        protected void SuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        protected void ErrorMessage(string message)
        {
            TempData["Error"] = message;
        }
        protected void WarningMessage(string message)
        {
            TempData["Warning"] = message;
        }

        protected void InfoMessage(string message)
        {
            TempData["Info"] = message;
        }
        //  public override async Task OnActionExecutionAsync(
        //ActionExecutingContext context,
        //ActionExecutionDelegate next)
        //  {
        //      var skipPermission =
        // context.ActionDescriptor.EndpointMetadata
        // .OfType<SkipPermissionAttribute>()
        // .Any();

        //      if (skipPermission)
        //      {
        //          await next();
        //          return;
        //      }
        //      if (User.Identity == null ||
        //          !User.Identity.IsAuthenticated)
        //      {
        //          await next();
        //          return;
        //      }

        //      var userId = _userManager.GetUserId(User);

        //      var controller =
        //          context.RouteData.Values["controller"]?.ToString();

        //      var action =
        //          context.RouteData.Values["action"]?.ToString();


        //      bool hasPermission =
        //          await _permissionService.HasPermission(
        //              userId!,
        //              controller!,
        //              action!);

        //      if (!hasPermission)
        //      {
        //          context.Result =
        //              new RedirectToActionResult(
        //                  "AccessDenied",
        //                  "Account",
        //                  null);

        //          return;
        //      }

        //      await next();
        //  }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }
            var skipPermission = context.ActionDescriptor.EndpointMetadata.OfType<SkipPermissionAttribute>().Any();

            if (skipPermission)
            {
                await next();
                return;
            }

            var userId = _userManager.GetUserId(User);

            var controller = context.RouteData.Values["controller"]?.ToString();

            var action = context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
            {
                await next();
                return;
            }

            var ignoredControllers = new[]
            {
                "Account",
                "Dashboard",
                "Home"
            };

            if (ignoredControllers.Contains(controller))
            {
                await next();
                return;
            }

            bool hasPermission =await _permissionService.HasPermission(userId!,controller,action,context.ActionArguments);

            if (!hasPermission)
            {
                context.Result =new RedirectToActionResult("AccessDenied","Account",null);

                return;
            }

            await next();
        }
        public async Task<SchoolMaster> GetSchoolInfo()
        {
            return await _context.SchoolMasters.AsNoTracking().FirstOrDefaultAsync();
        }
        [SkipPermission]
        public List<DataListItem> GetDataListItems(string dataListName)
        {
            var dataListId = _context.DataLists
                .Where(x => x.DataListName == dataListName && x.IsActive == true)
                .Select(x => x.DataListId)
                .FirstOrDefault();

            if (dataListId == 0) return new List<DataListItem>();

            return _context.DataListItems
                .Where(x => x.DataListId == dataListId)
                .ToList();
        }

        public async Task RetryFailedEmails()
        {
            var failedEmails = await _context.EmailLogs
                .Where(x => x.Status == EmailStatus.Failed
                            && x.RetryCount < 3)
                .ToListAsync();

            foreach (var email in failedEmails)
            {
                await _email.SendEmailAsync(
                    email.ModuleName,
                    email.ReferenceId,
                    email.ToEmail,
                    email.Subject,
                    email.Body);
            }
        }
        public async Task<long> QueueEmailAsync(SendEmailRequest request)
        {
            var log = new EmailLog
            {
                ModuleName = request.ModuleName,
                ReferenceId = request.ReferenceId,
                ToEmail = request.ToEmail,
                Subject = request.Subject,
                Body = request.Body,
                Status = EmailStatus.Pending
            };

            _context.EmailLogs.Add(log);

            await _context.SaveChangesAsync();

            return log.EmailLogId;
        }
        public List<StaffMaster> GetTeacherList()
        {
            return _context.StaffMasters

                .Where(x => x.IsActive)

                .OrderBy(x => x.FirstName)

                .Select(x => new StaffMaster
                {
                    StaffId = x.StaffId,

                    FirstName = x.FullName
                })

                .ToList();
        }
        public List<Batches> GetBatchList()
        {
            return  _context.Batches

                .Where(x => x.IsActive)

                .OrderByDescending(x => x.BatchId)

                .Select(x => new Batches
                {
                    BatchId = x.BatchId,
                   AcademicYear = x.AcademicYear
                })

                .ToList();
        }
        protected string CurrentUserName
        {
            get
            {
                var currentUser = HttpContext?.Session?.GetCurrentUser();
                return currentUser?.UserName ?? User?.Identity?.Name ?? string.Empty;
            }
        }

        protected UserSessionVM GetCurrentUser()
        {
            
            return HttpContext?.Session?.GetCurrentUser();
        }
    }


}  

