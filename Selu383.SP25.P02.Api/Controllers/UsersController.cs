using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only Admins should access this
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ GET ALL USERS
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = _userManager.GetRolesAsync(user).Result.ToArray()
            }).ToList();

            return Ok(userDtos);
        }

        // ✅ GET USER BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray()
            });
        }

        // ✅ CREATE NEW USER (Admin Only)
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            // ✅ Validate input (Empty Username, Empty Roles, No Password)
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                return BadRequest("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Password is required.");
            }

            if (model.Roles == null || model.Roles.Length == 0)
            {
                return BadRequest("At least one role is required.");
            }

            // ✅ Check if the username already exists
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                return BadRequest("Username already exists.");
            }

            var user = new User
            {
                UserName = model.UserName
            };

            // ✅ Create user with password
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            // ✅ Validate Roles
            foreach (var role in model.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return BadRequest($"Role '{role}' does not exist.");
                }
            }

            // ✅ Assign roles to user
            await _userManager.AddToRolesAsync(user, model.Roles);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = model.Roles
            });
        }

        // ✅ UPDATE USER ROLES
        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateUserRoles(int id, [FromBody] string[] roles)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new roles
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return BadRequest($"Role '{role}' does not exist.");
                }
            }
            await _userManager.AddToRolesAsync(user, roles);

            return Ok(new { message = $"User {user.UserName} roles updated successfully!" });
        }

        // ✅ DELETE USER
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            await _userManager.DeleteAsync(user);
            return Ok(new { message = $"User {user.UserName} deleted successfully!" });
        }
    }
}
