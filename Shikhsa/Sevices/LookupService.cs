using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Repositories;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using System.Linq.Expressions;
using System.Security.Claims;

public class LookupService 
{
    private readonly LookupRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    public LookupService(
        LookupRepository repository,
        UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _repository = repository;
        _userManager = userManager;
        _context = context;
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
    public async Task BindAsync(BaseFilterVM vm,ClaimsPrincipal user,string changedBy = "")
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
    public async Task PopulateAsync<T>(T model) where T : class
    {
        if (model == null)
            return;

        var properties = typeof(T)
            .GetProperties()
            .Select(x => new
            {
                Property = x,
                Attribute = x.GetCustomAttributes(typeof(MapNameAttribute), true)
                            .Cast<MapNameAttribute>()
                            .FirstOrDefault()
            })
            .Where(x => x.Attribute != null)
            .ToList();

        foreach (var item in properties)
        {
            var attribute = item.Attribute!;
            var id = item.Property.GetValue(model);

            if (id == null)
                continue;

            var nameProperty =
                typeof(T).GetProperty(attribute.NameProperty);

            if (nameProperty == null)
                continue;

            string? name = null;

            // DataListItem
            if (!string.IsNullOrWhiteSpace(attribute.DataListName))
            {
                name = await _context.DataListItems
                    .AsNoTracking()
                    .Where(x =>
                        x.DataListItemId == Convert.ToInt32(id) &&
                        x.DataList != null &&
                        x.DataList.DataListName ==
                            attribute.DataListName)
                    .Select(x => x.DataListItemText)
                    .FirstOrDefaultAsync();
            }

            // Other table e.g. Batch
            else if (attribute.LookupType != null)
            {
                name = await GetLookupNameAsync(
                    attribute.LookupType,
                    attribute.LookupKeyProperty!,
                    attribute.LookupNameProperty!,
                    Convert.ToInt32(id));
            }

            nameProperty.SetValue(model, name);
        }
    }
    private async Task<string?> GetLookupNameAsync(
    Type entityType,
    string keyProperty,
    string nameProperty,
    int id)
    {
        var method = typeof(LookupService)
            .GetMethod(
                nameof(GetLookupNameGenericAsync),
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)!;

        var genericMethod = method.MakeGenericMethod(entityType);

        var task = (Task)genericMethod.Invoke(
            this,
            new object[]
            {
            keyProperty,
            nameProperty,
            id
            })!;

        await task;

        return task
            .GetType()
            .GetProperty("Result")?
            .GetValue(task)?
            .ToString();
    }
    private async Task<string?> GetLookupNameGenericAsync<TEntity>(
    string keyProperty,
    string nameProperty,
    int id)
    where TEntity : class
    {
        return await _context.Set<TEntity>()
            .AsNoTracking()
            .Where(x =>
                EF.Property<int>(x, keyProperty) == id)
            .Select(x =>
                EF.Property<string>(x, nameProperty))
            .FirstOrDefaultAsync();
    }
    private static LambdaExpression BuildWhereExpression(Type entityType,string propertyName,int value)
    {
        var parameter =
            Expression.Parameter(entityType, "x");

        var property =
            Expression.Property(parameter, propertyName);

        var constant =
            Expression.Constant(value, property.Type);

        var body =
            Expression.Equal(property, constant);

        return Expression.Lambda(body, parameter);
    }

    private static LambdaExpression BuildSelectExpression(
        Type entityType,
        string propertyName)
    {
        var parameter =
            Expression.Parameter(entityType, "x");

        var property =
            Expression.Property(parameter, propertyName);

        return Expression.Lambda(property, parameter);
    }
}