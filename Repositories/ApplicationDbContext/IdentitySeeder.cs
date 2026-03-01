using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using System.Security.Cryptography;
using System.Text;

namespace Repositories.ApplicationDbContext
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            if (!await context.Users.AnyAsync())
            {
                List<User> userList = new List<User>
                {
                    new User
                    {
                        UserName = "admin@example.com",
                        PasswordHash = HashPassword("Admin123@"),
                        Role = "Admin"
                    },
                    new User
                    {
                        UserName = "test1@example.com",
                        PasswordHash = HashPassword("User123@"),
                        Role = "User"
                    }
                };

                await context.Users.AddRangeAsync(userList);
                await context.SaveChangesAsync();
            }
            else
            {
                // Ensure existing admin user has Admin role (e.g. after adding Role column)
                var admin = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin@example.com");
                if (admin != null && admin.Role != "Admin")
                {
                    admin.Role = "Admin";
                    await context.SaveChangesAsync();
                }
            }
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }
    }
}