namespace Repositories.Models
{
    public class ProductSize
    {
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public int SizeId { get; set; }
        public Size Size { get; set; }

        public int Quantity { get; set; }

    }
}
