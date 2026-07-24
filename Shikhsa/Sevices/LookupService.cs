using Microsoft.AspNetCore.Identity;
using Shikhsa.Models;
using Shikhsa.Repositories;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using System.Security.Claims;

public class LookupService 
{
    private readonly LookupRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public LookupService(
        LookupRepository repository,
        UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    //public async Task BindAsync(BaseFilterVM vm, ClaimsPrincipal user)
    //{
    //    // Batch
    //    vm.Batches = await _repository.GetBatchesAsync();

    //    bool isAdmin =
    //           user.IsInRole("Admin")
    //        || user.IsInRole("Principal")
    //        || user.IsInRole("Developer")
    //        || user.IsInRole("YN IT Solutions");

    //    if (isAdmin)
    //    {
    //        await LoadAdmin(vm);
    //    }
    //    else
    //    {
    //        await LoadTeacher(vm, user);
    //    }

    //    await LoadClasses(vm);

    //    await LoadSections(vm);
    //}
    public async Task BindAsync(
    BaseFilterVM vm,
    ClaimsPrincipal user,
    string changedBy = "")
    {
        vm.Batches = await _repository.GetBatchesAsync();

        bool isAdmin =
            user.IsInRole("Admin") ||
            user.IsInRole("Principal") ||
            user.IsInRole("Developer") ||
            user.IsInRole("YN IT Solutions");

        if (isAdmin)
            await LoadAdmin(vm);
        else
            await LoadTeacher(vm, user);

        switch (changedBy)
        {
            case "Batch":
                vm.ClassId = 0;
                vm.SectionId = 0;
                break;

            case "Staff":
                vm.ClassId = 0;
                vm.SectionId = 0;
                break;

            case "Class":
                vm.SectionId = 0;
                break;
        }

        await LoadClasses(vm);
        await LoadSections(vm);
    }
    private async Task LoadAdmin(BaseFilterVM vm)
    {
        vm.Staffs = await _repository.GetStaffsAsync();

        if (vm.StaffId > 0)
        {
            await LoadTeacherAssignment(vm);
        }
    }

    //private async Task LoadTeacher(BaseFilterVM vm, ClaimsPrincipal user)
    //{
    //    var userId = _userManager.GetUserId(user);

    //    if (string.IsNullOrWhiteSpace(userId))
    //        return;

    //    var staff = (await _repository.GetStaffsAsync())
    //        .FirstOrDefault(x => x.UserId == userId);

    //    if (staff == null)
    //        return;

    //    vm.StaffId = staff.StaffId;

    //    vm.Staffs.Add(staff);

    //    vm.LockStaff = true;

    //    await LoadTeacherAssignment(vm);
    //}

    private async Task LoadTeacherAssignment(BaseFilterVM vm)
    {
        if (vm.StaffId <= 0)
            return;

        if (vm.BatchId <= 0)
            return;

        var assignment = await _repository.GetClassTeacherAsync(
            vm.StaffId,
            vm.BatchId);

        if (assignment == null)
            return;

        vm.BatchId = assignment.BatchId;
        vm.ClassId = assignment.ClassId;
        vm.SectionId = assignment.SectionId;

        vm.LockBatch = true;
        vm.LockClass = true;
        vm.LockSection = true;
    }

    private async Task LoadClasses(BaseFilterVM vm)
    {
        if (vm.BatchId <= 0 || vm.StaffId <= 0)
            return;

        vm.Classes = await _repository.GetClassesAsync(
            vm.BatchId,
            vm.StaffId);
    }

    private async Task LoadSections(BaseFilterVM vm)
    {
        if (vm.BatchId <= 0)
            return;

        if (vm.ClassId <= 0)
            return;

        if (vm.StaffId <= 0)
            return;

        vm.Sections = await _repository.GetSectionsAsync(
            vm.BatchId,
            vm.ClassId,
            vm.StaffId);
    }
    private async Task LoadTeacher(BaseFilterVM vm, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var staff = await _repository.GetStaffByUserIdAsync(userId);

        if (staff == null)
            return;

        vm.StaffId = staff.StaffId;

        vm.Staffs.Add(staff);

        vm.LockStaff = true;

        await LoadTeacherAssignment(vm);
    }

}