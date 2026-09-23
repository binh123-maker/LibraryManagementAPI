namespace WebApplication1.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = "Borrowing";

        public User? User { get; set; }
    }
}