using business_logic.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace data_access.Identity
{
    public static class IdentitySeeder
    {
        private const string DefaultAdminUserName = "admin";
        private const string DefaultAdminEmail = "admin@gmail.com";

        public static async Task SeedAsync(IServiceProvider services, SeedAdminSettings? seedAdminSettings)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var role in UserRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var admin = await userManager.FindByNameAsync(DefaultAdminUserName)
                ?? await userManager.FindByEmailAsync(DefaultAdminEmail);
            if (admin != null)
            {
                return;
            }

            var defaultAdminPassword = seedAdminSettings?.Password;
            if (string.IsNullOrWhiteSpace(defaultAdminPassword))
            {
                throw new InvalidOperationException(
                    $"'{SeedAdminSettings.SectionName}:{nameof(SeedAdminSettings.Password)}' is not configured, so the default admin '{DefaultAdminEmail}' cannot be created.");
            }

            admin = new IdentityUser { UserName = DefaultAdminUserName, Email = DefaultAdminEmail, EmailConfirmed = true };
            var createAdminResult = await userManager.CreateAsync(admin, defaultAdminPassword);
            if (!createAdminResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Default admin '{DefaultAdminEmail}' was not created: {string.Join(" ", createAdminResult.Errors.Select(error => error.Description))}");
            }

            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
        }
    }
}
