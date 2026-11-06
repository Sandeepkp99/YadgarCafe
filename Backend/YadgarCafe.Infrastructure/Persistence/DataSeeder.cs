using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YadgarCafe.Common.Constants;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static async Task InitialiseAsync(IServiceProvider services, bool migrate)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");

            if (migrate)
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }

            await SeedRolesAsync(scope.ServiceProvider);
            await SeedAdminAsync(scope.ServiceProvider, logger);
        }

        private static async Task SeedRolesAsync(IServiceProvider sp)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var role in new[] { Roles.Admin, Roles.Staff, Roles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        /// <summary>
        /// Creates the first Admin from configuration (Seed:AdminEmail / Seed:AdminPassword).
        /// Without those settings nothing is created, so no default credentials ever ship.
        /// </summary>
        private static async Task SeedAdminAsync(IServiceProvider sp, ILogger logger)
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var email = config["Seed:AdminEmail"];
            var password = config["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Seed:AdminEmail / Seed:AdminPassword are not set - no admin account was created.");
                return;
            }

            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.FindByEmailAsync(email) is not null)
                return;

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = config["Seed:AdminName"] ?? "Administrator",
                UserType = UserType.Admin,
                EmailConfirmed = true
            };

            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                logger.LogError("Admin seeding failed: {Errors}", string.Join(" ", result.Errors.Select(e => e.Description)));
                return;
            }

            await users.AddToRoleAsync(admin, Roles.Admin);
            logger.LogInformation("Seeded admin account {Email}.", email);
        }
    }
}
