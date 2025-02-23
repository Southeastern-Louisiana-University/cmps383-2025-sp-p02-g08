using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Ensure Correct Connection String (Use Environment Variable in Azure)
            var connectionString = builder.Configuration.GetConnectionString("DataContext") ??
                                   Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DataContext") ??
                                   throw new InvalidOperationException("Database connection string not found.");

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Identity Authentication
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthorization();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            // Enable Swagger (API Documentation)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Theater API",
                    Version = "v1",
                    Description = "API for managing theaters, users, and authentication."
                });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            // ENSURE DATABASE MIGRATION & SEEDING
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<DataContext>();

                try
                {
                    Console.WriteLine("Applying Migrations...");
                    await db.Database.MigrateAsync();  // Ensure DB is up to date
                    Console.WriteLine("Migrations Applied Successfully.");

                    Console.WriteLine("Seeding Users & Roles...");
                    await SeedUsersAndRoles.EnsureSeededAsync(services);  // Seed Users & Roles
                    Console.WriteLine("Seeding Theaters...");
                    SeedTheaters.Initialize(scope.ServiceProvider); // Seed Theaters
                    Console.WriteLine("Seeding Completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? ERROR: Database migration/seed failed - {ex.Message}");
                }
            }

            //  Enable Middleware
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(x => { x.MapControllers(); });

            //  Enable Swagger UI (Make sure it's accessible in production)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Theater API V1");
                c.RoutePrefix = string.Empty;  // Set Swagger UI to root URL
            });

            //  Fix SPA Routing Issues
            app.UseStaticFiles();
            if (app.Environment.IsDevelopment())
            {
                app.UseSpa(x => { x.UseProxyToSpaDevelopmentServer("http://localhost:5173"); });
            }
            else
            {
                Console.WriteLine(" Running in Production Mode - API Endpoints Only");
                app.MapFallbackToFile("/index.html");
            }

            app.Run();
        }
    }
}
