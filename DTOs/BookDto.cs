namespace WebApplication1.DTOs
{
    public class BookDto
    {
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Publisher { get; set; } = string.Empty;

        public int PublishYear { get; set; }

        public int Quantity { get; set; }

        public int CategoryId { get; set; }
    }
}