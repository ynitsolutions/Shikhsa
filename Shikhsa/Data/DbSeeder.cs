//namespace Shikhsa.Data
//{
//    public class DbSeeder
//    {
//    }
//}
// Data/DbSeeder.cs
using Microsoft.AspNetCore.Identity;
using Shikhsa.Models;

namespace Shikhsa.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Only seed the first super admin role & user
            // Everything else is managed via UI

            string superAdminRole = "YN IT Solutions";
            string superAdminEmail = "admin@ynitsolutions.com";

            // Create super admin role if not exists
            if (!await roleManager.RoleExistsAsync(superAdminRole))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = superAdminRole,
                    Description = "System Administrator",
                    IsActive = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = "System"
                });
            }

            // Create super admin user if not exists
            if (await userManager.FindByEmailAsync(superAdminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = "YN IT Solutions Admin",
                    Department = "IT",
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, superAdminRole);
            }
        }
    }
}
