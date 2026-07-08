using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using System.Text.Json;

namespace Shikhsa.Controllers
{
    public class StaffController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly StaffRepository _repository;
        private readonly StaffUserRepository _repositoryUser;
        private readonly IWebHostEnvironment _environment;
        private readonly FileUploadHelper _uploadHelper;

        public StaffController(
            ApplicationDbContext context,
            IConfiguration configuration,
            StaffRepository repository,
            UserManager<ApplicationUser> userManager,
 PermissionService permissionService, IWebHostEnvironment env, EmailService email,
            IWebHostEnvironment environment, FileUploadHelper uploadHelper, StaffUserRepository repositoryUser, RoleManager<ApplicationRole> roleManager) : base(userManager, permissionService, context, email)
        {
            _context = context;
            _configuration = configuration;
            _repository = repository;
            _environment = environment;
            _uploadHelper = uploadHelper;
            _repositoryUser = repositoryUser;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Staffs(StaffFilterVM filter)
        {
            var data = await _repository.GetStaffList(filter);

            ViewBag.Filter = filter;

            return View(data);
        }
        public async Task<IActionResult> SaveStaffs(long? id=0)
        {
            await LoadDropdowns();
            StaffMaster model = new StaffMaster();
            if (id == 0)
            {
                

                model.JoiningDate = DateTime.Today;

                model.IsActive = true;
            }
            else
            {
                model = await _repository.GetStaffById((long)id);

                if (model == null)
                {
                    ErrorMessage("Record not found.");

                    return RedirectToAction("Staffs");
                }
            }
            return View(model);
        }
        public async Task<IActionResult> DeleteStaffs(long id)
        {
            var result = await _repository.DeleteStaff(id);

            if (result.Status == 1)
               SuccessMessage( result.Message);
            else
                ErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }
        private async Task LoadDropdowns()
        {
            ViewBag.Gender =
                 GetDataListItems("Gender");

            ViewBag.BloodGroup =
                 GetDataListItems("Blood Group");

            ViewBag.Religion =
                 GetDataListItems("Religion");

            ViewBag.Category =
                 GetDataListItems("Category");

            ViewBag.Department =
                 GetDataListItems("Department");

            ViewBag.Designation =
                 GetDataListItems("Designation");

            ViewBag.StaffType =
                 GetDataListItems("Staff Type");

            ViewBag.EmploymentType =
                 GetDataListItems("Employment Type");

            ViewBag.Shifts =
                 GetDataListItems("Shift");

            ViewBag.Banks =
                 GetDataListItems("Bank");

            ViewBag.EmployeeStatus =
                 GetDataListItems("Employee Status");

            ViewBag.LeaveGroup =
                 GetDataListItems("Leave Group");

            ViewBag.Relationship =
                 GetDataListItems("Relationship");

            ViewBag.Degree =
                 GetDataListItems("Degree");

            ViewBag.Grade =
                 GetDataListItems("Grade");

            ViewBag.DocumentType =
                 GetDataListItems("Document Type");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStaffs(StaffMaster model,
    string AcademicJson,
    string ExperienceJson,
    string DocumentJson
    )
        {
            model.Academics =
    string.IsNullOrWhiteSpace(AcademicJson)
    ? new List<StaffAcademic>()
    : JsonSerializer.Deserialize<List<StaffAcademic>>(AcademicJson);

            model.Experiences =
                string.IsNullOrWhiteSpace(ExperienceJson)
                ? new List<StaffExperience>()
                : JsonSerializer.Deserialize<List<StaffExperience>>(ExperienceJson);

            model.Documents =
                string.IsNullOrWhiteSpace(DocumentJson)
                ? new List<StaffDocument>()
                : JsonSerializer.Deserialize<List<StaffDocument>>(DocumentJson);

            //model.EmergencyContacts =
            //    string.IsNullOrWhiteSpace(EmergencyJson)
            //    ? new List<StaffEmergencyContact>()
            //    : JsonSerializer.Deserialize<List<StaffEmergencyContact>>(EmergencyJson);

            //model.LeaveSetting =
            //    string.IsNullOrWhiteSpace(LeaveJson)
            //    ? new StaffLeaveSetting()
            //    : JsonSerializer.Deserialize<StaffLeaveSetting>(LeaveJson);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
    .Where(x => x.Value.Errors.Any())
    .Select(x => new
    {
        Field = x.Key,
        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
    })
    .ToList();
                await LoadDropdowns();

                return View("SaveStaffs", model);
            }
            

            if (model.PhotoFile != null)
            {
                model.PhotoPath =
                    await _uploadHelper.UploadFile(
                        model.PhotoFile,
                        "Staff","Photo");
            }

            if (model.SignatureFile != null)
            {
                model.SignaturePath =
                    await _uploadHelper.UploadFile(
                        model.SignatureFile,
                        "Staff","Signature");
            }

            if (model.AadhaarFile != null)
            {
                string aadhaarPath =
                    await _uploadHelper.UploadFile(
                        model.AadhaarFile,
                        "Staff","Aadhaar");

                //model.Documents.Add(new StaffDocument
                //{
                //    DocumentTypeId = 1,
                //    FilePath = aadhaarPath,
                //    OriginalFileName = model.AadhaarFile.FileName
                //});
            }

            string xml =
                XmlHelper.Serialize(model);

            var result =
                await _repository.SaveStaff(xml);

            if (result.Status == 1)
            {
                SuccessMessage(result.Message);

                return RedirectToAction("Staffs");
            }

            ErrorMessage(result.Message);

            await LoadDropdowns();

            return View("SaveStaffs", model);
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> UploadStaffFile(
    IFormFile file,
    string fileType)
        {
            try
            {
                fileType = (fileType ?? "").Replace("\r", "").Replace("\n", "").Trim();

                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    fileType = fileType.Replace(c.ToString(), "");
                }

                fileType = fileType.Replace(" ", "");
                string path = await _uploadHelper.UploadFile(
                    file,
                    "Staff",
                    fileType);

                return Json(new
                {
                    success = true,
                    filePath = path,
                    fileName = file.FileName
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        #region Index
        [HttpGet]
        public async Task<ActionResult> StaffLogins()
        {
            //var model = await _repositoryUser.GetPageData();
            ViewBag.StaffList = await _context.StaffMasters

               .Where(x => x.UserId == null)

               .OrderBy(x => x.FirstName).ThenBy(x => x.MiddleName).ThenBy(x => x.LastName)

               .Select(x => new SelectListItem
               {
                   Value = x.StaffId.ToString(),
                   Text = x.FirstName + " " + x.MiddleName + " " + x.LastName
               }).ToListAsync();


            ViewBag.RoleList = await _roleManager.Roles

                .OrderBy(x => x.Name)

                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.Name
                }).ToListAsync();
            return View();
        }
        [SkipPermission]
        public async Task<IActionResult> GetStaffUserList()
        {
            var data = await _repositoryUser.GetStaffUserListAsync();
            return Json(new { success = true, data = data });
        }

        #endregion

        #region Save(Create/Update)

        [HttpPost]
        public async Task<IActionResult> SaveStaffLogins(StaffUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResponseModel
                {
                    Status = 0,
                    Message = "Please fill all required fields."
                });
            }

            ResponseModel response;

            if (string.IsNullOrEmpty(model.UserId))
            {
                response = await _repositoryUser.CreateUser(model.StaffId, model.RoleId);
            }
            else
            {
                response = await _repositoryUser.UpdateRole(model.UserId, model.RoleId);
            }

            return Json(response);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> SaveStaffLogins(string userId)
        {
            var data = await _repositoryUser.GetUserForEdit(userId);

            if (data == null)
            {
                return Json(new ResponseModel
                {
                    Status = 0,
                    Message = "Record not found."
                });
            }

            return Json(data);
        }

        #endregion

        #region Change Password

        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResponseModel
                {
                    Status = 0,
                    Message = "Invalid Data."
                });
            }

            bool isAdmin = User.IsInRole("Admin");

            var result = await _repositoryUser.ChangePassword(model, isAdmin);

            return Json(result);
        }

        #endregion

        #region Clear
        [SkipPermission]
        [HttpGet]
        public IActionResult Clear()
        {
            return Json(new ResponseModel
            {
                Status = 1
            });
        }
        [SkipPermission]
        public async Task<IActionResult> GetStaffDropdown()
        {
            var list = await _repositoryUser.GetStaffDropdown();

            return Json(list);
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string oldRoleId, string newRoleId)
        {
            var response = await _repositoryUser.UpdateUserRole(userId, oldRoleId, newRoleId);
            return Json(response);
        }
        #endregion
    }
}

