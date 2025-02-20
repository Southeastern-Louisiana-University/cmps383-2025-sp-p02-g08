using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Features.UserRoles
{
    public class UserRole : IdentityUserRole<int>
    {
        public User User { get; set; } = default!;
        public Role Role { get; set; } = default!;
    }
}
