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
        public string Address { get; set; }
        public string Role { get; set; } = "User";
        public ICollection<Order> Orders { get; set; }
        public Cart Cart { get; set; }

        public ICollection<Conversation> Conversations { get; set; }
    }
}
