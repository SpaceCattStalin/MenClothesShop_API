namespace Repositories.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Total { get; set; }
        public DateTime Created { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public User User { get; set; }

    }
}
