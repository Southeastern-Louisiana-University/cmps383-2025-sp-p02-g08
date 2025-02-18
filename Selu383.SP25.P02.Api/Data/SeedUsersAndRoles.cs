using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;
using System;
using System.Threading.Tasks;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedUsersAndRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            // ✅ Ensure Roles Exist
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }

            // ✅ Ensure Users Exist
            var users = new (string UserName, string Role)[] {
                ("bob", "User"),
                ("sue", "User"),
                ("galkadi", "Admin")
            };

            foreach (var (userName, role) in users)
            {
                var user = await userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    user = new User { UserName = userName };
                    await userManager.CreateAsync(user, "Password123!");
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
