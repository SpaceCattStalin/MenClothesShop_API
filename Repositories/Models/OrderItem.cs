namespace Repositories.Models
{
    // Version 1
    public class OrderItem
    {
        public int ProductVariantId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public int SizeId { get; set; }
        public Size Size { get; set; }
        public Order Order { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
