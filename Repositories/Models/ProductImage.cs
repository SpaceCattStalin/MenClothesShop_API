namespace Repositories.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int VariantId { get; set; }
        public ProductVariant Variant { get; set; }
    }
}
