namespace Repositories.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Usename { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;

    }
}
