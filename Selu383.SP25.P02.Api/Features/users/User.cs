using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.UserRoles;

namespace Selu383.SP25.P02.Api.Features.Users
{
    public class User : IdentityUser<int>
    {
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
