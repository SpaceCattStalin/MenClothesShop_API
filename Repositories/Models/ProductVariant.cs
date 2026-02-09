namespace Repositories.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int MainProductId { get; set; }
        public int ColorCode { get; set; }
        public Color Color { get; set; }
        public ICollection<ProductSize> Sizes { get; set; }
        public int TotalQuantity { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public Product MainProduct { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
