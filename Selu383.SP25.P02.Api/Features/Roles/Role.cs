using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.UserRoles;

namespace Selu383.SP25.P02.Api.Features.Roles
{
    public class Role : IdentityRole<int>
    {
        public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    }
}
