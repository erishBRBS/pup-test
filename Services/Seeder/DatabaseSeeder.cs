using Microsoft.EntityFrameworkCore;
using UserManagement.API.Data;
using UserManagement.API.Models;

namespace UserManagement.API.Services.Seeder
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
        {
            await context.Database.MigrateAsync();

            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role { Id = 1, RoleName = "Admin" },
                    new Role { Id = 2, RoleName = "User" }
                );

                await context.SaveChangesAsync();
            }

            var adminUsername = configuration["SeedAdmin:Username"];
            var adminPassword = configuration["SeedAdmin:Password"];
            var adminFirstName = configuration["SeedAdmin:FirstName"] ?? "System";
            var adminLastName = configuration["SeedAdmin:LastName"] ?? "Admin";

            if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var adminExists = await context.Users.AnyAsync(x => x.Username == adminUsername);

            if (!adminExists)
            {
                var admin = new User
                {
                    Username = adminUsername,
                    Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    RoleId = 1,
                    Status = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}