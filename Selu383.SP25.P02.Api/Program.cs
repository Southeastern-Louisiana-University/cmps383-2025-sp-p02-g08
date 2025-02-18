using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ? Add Database Context
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")
                ?? throw new InvalidOperationException("Connection string 'DataContext' not found.")));

            // ? Add Identity Authentication
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            })
               .AddEntityFrameworkStores<DataContext>()
               .AddDefaultTokenProviders();

            // ? Configure Authentication Cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // ? Add Controllers
            builder.Services.AddControllers();

            // ? Add Swagger (OpenAPI)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Theater API",
                    Version = "v1",
                    Description = "ASP.NET Core API for managing theaters, users, and authentication.",
                });

                // ? Add JWT Authentication in Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer {your token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // ? SEED DATABASE ON STARTUP
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<DataContext>();
                await db.Database.MigrateAsync();  // Ensure database is up to date

                 SeedTheaters.Initialize(scope.ServiceProvider);  //  SEED THEATERS
                await SeedUsersAndRoles.Initialize(services);  //  SEED USERS & ROLES
            }

            // ? Enable Swagger UI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Theater API V1");
                    c.RoutePrefix = ""; // Set Swagger UI as default page
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
