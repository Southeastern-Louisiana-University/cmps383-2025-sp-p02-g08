using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.UserRoles;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Selu383.SP25.P02.Api.Data
{
    public class DataContext : IdentityDbContext<User, Role, int,
        IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var userRoleBuilder = builder.Entity<UserRole>();

            userRoleBuilder.HasKey(x => new { x.UserId, x.RoleId });

            userRoleBuilder.HasOne(navigationExpression: x => x.Role)
                .WithMany(navigationExpression: x => x.Users)
                .HasForeignKey(x => x.RoleId);

            userRoleBuilder.HasOne(navigationExpression: x => x.User)
                .WithMany(navigationExpression: x => x.Roles)
                .HasForeignKey(x => x.UserId);

        }

        public DbSet<Theater> Theaters { get; set; }
    }
}
