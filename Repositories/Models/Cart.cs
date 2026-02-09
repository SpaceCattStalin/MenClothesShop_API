namespace Repositories.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CartItem> Items { get; set; }
    }
}
