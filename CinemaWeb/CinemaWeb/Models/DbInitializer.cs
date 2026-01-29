using CinemaWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace CinemaWeb.Models
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Створюю ролі
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Створюю адміністратора
            var adminEmail = "admin@cinemaweb.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = "admin@cinemaweb.com",
                    Email = adminEmail,
                    FullName = "Головний Адміністратор",
                    EmailConfirmed = true
                };

                // Створюю користувача з паролем
                var result = await userManager.CreateAsync(newAdmin, "AdminPass123!");

                if (result.Succeeded)
                {
                    // Присвоюємо йому роль Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
