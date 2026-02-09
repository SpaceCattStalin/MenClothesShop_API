namespace Repositories.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<ProductVariant> Variants { get; set; }
        public int TotalQuantity { get; set; }
        public int CatId { get; set; }
        public Category Category { get; set; }
    }
}
