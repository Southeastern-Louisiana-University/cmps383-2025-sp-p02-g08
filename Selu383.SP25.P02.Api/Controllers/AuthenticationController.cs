using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Selu383.SP25.P02.Api.Features.Users;
using System.Threading.Tasks;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //  LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return BadRequest("Invalid username or password.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                return BadRequest("Invalid username or password.");
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray()
            });
        }

        //  GET LOGGED-IN USER DETAILS
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray()
            });
        }

        //  LOGOUT
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        //  CREATE NEW USER (Admin Only)
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                return BadRequest("Username already exists.");
            }

            var user = new User
            {
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign roles
            foreach (var role in model.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return BadRequest($"Role '{role}' does not exist.");
                }

                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = model.Roles
            });
        }
    }
}
