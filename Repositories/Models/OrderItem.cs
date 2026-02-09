namespace Repositories.Models
{
    public class OrderItem
    {
        public int ProductVariantId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }

        public Order Order { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
