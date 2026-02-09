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
                        PasswordHash = HashPassword("Admin123@")
                    },
                    new User
                    {
                        UserName = "test1@example.com",
                        PasswordHash = HashPassword("User123@")
                    }
                };

                await context.Users.AddRangeAsync(userList);
                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password)
        {
            // Using PBKDF2 with HMACSHA256
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            // Combine salt and hash
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }
    }
}