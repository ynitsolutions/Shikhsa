using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;

//using Shikhsa.Middlewares;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Repositories;
using Shikhsa.Repository;
using Shikhsa.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(   
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})

.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.HttpOnly = true;
    options.Cookie.Name = ".AspNetCore.Identity.Application";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});
/* Sessions */
//builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();



// ─────────────────────────────────────────────────────────────
// SERVICES
// ─────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<StudentReportRepository>();
builder.Services.AddScoped<ExamRepository>();
builder.Services.AddScoped<StaffRepository>();
builder.Services.AddScoped<GradingRepository>();
builder.Services.AddScoped<ClassTeacherRepository>();
builder.Services.AddScoped<StaffAttendanceRepository>();
builder.Services.Configure<EmailSettings>(
builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<FileUploadHelper>();
builder.Services.AddScoped<StaffUserRepository>();
builder.Services.AddScoped<FeeHeadingRepository>();
builder.Services.AddScoped<LookupRepository>();
builder.Services.AddScoped<NotificationTemplateRepository>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();


// ─────────────────────────────────────────────────────────────
// SEED — SUPER ADMIN ONLY (one-time bootstrap)
// Everything else is created via the management UI and stored in DB
// ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    try
    {
        // ── Super Admin Role ──────────────────────────────
        // This is the only role seeded — all other roles are
        // created from the Roles Management page in the UI
        const string superAdminRole = "YN IT Solutions";

        if (!await roleManager.RoleExistsAsync(superAdminRole))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = superAdminRole,
                Description = "System Super Administrator",
                IsActive = true,
                CreatedOn = DateTime.Now,
                CreatedBy = "System"
            });
        }

        // ── Super Admin User ──────────────────────────────
        // This is the only user seeded — all other users are
        // created from the Users Management page in the UI
        const string superAdminEmail = "admin@ynitsolutions.com";

        if (await userManager.FindByEmailAsync(superAdminEmail) == null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "YN IT Solutions Admin",
                Department = "IT",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, "Admin@12345");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, superAdminRole);
        }

        // ── NOTE ──────────────────────────────────────────
        // Roles      → managed via /Roles
        // Users      → managed via /Users
        // Menus      → managed via /Menus
        // Menu Rights → managed via /MenuRights
        // All data lives in DB — nothing hardcoded beyond this point
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during initial seed.");
    }
}


// ─────────────────────────────────────────────────────────────
// HTTP PIPELINE
// ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // ← must be before UseAuthorization
//app.UseMiddleware<PermissionMiddleware>();
app.UseAuthorization();


// ─────────────────────────────────────────────────────────────
// ROUTES
// ─────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapRazorPages();

app.Run();