namespace WebApplication1.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Navigation property (1 Category có nhiều Product)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}