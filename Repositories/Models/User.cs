using Microsoft.AspNetCore.Identity;
using System.Security.Principal;

namespace Repositories.Models
{
    //public class User : IdentityUser
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<Order> Orders { get; set; }
        public Cart Cart { get; set; }
    }
}
