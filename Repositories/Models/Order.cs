namespace Repositories.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime Created { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public string To { get; set; }
        public Payment Payment { get; set; }
        public User User { get; set; }

    }
}
