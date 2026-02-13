using System.Collections.Generic;

namespace Repositories.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImgUri { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
