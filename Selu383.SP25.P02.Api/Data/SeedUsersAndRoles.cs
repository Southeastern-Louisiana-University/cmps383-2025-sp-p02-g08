using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedUsersAndRoles
    {
        public static async Task EnsureSeededAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roles = { "Admin", "User" };

            //  Ensure Roles Exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Role { Name = role });
                }
            }

            //  Ensure Users Exist
            await CreateUserIfNotExists(userManager, "galkadi", "Admin");
            await CreateUserIfNotExists(userManager, "bob", "User");
            await CreateUserIfNotExists(userManager, "sue", "User");
        }

        private static async Task CreateUserIfNotExists(UserManager<User> userManager, string username, string role)
        {
            if (await userManager.FindByNameAsync(username) == null)
            {
                var user = new User { UserName = username };
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
