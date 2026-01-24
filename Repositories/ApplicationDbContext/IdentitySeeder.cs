using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;

namespace Repositories.ApplicationDbContext
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();

            if (!userManager.Users.Any())
            {
                List<User> userList = new List<User>
                {
                    new User {UserName = "admin"},
                    new User {UserName = "test1"}
                };

                foreach (var user in userList)
                {
                    if (user.UserName == "admin")
                    {
                        await userManager.CreateAsync(user, "Admin123@");
                    }
                    else
                    {
                        await userManager.CreateAsync(user, "User123@");
                    }
                }
            }
        }
    }
}
