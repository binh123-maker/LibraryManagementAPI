namespace WebApplication1.DTOs
{
    public class BookDto
    {
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string? ISBN { get; set; }

        public string? Publisher { get; set; }

        public int PublishYear { get; set; }

        public int Quantity { get; set; }

        public int? AvailableQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public string? Location { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; } = 0;

        public int CategoryId { get; set; }
    }
}