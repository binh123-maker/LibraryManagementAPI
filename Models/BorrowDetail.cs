namespace WebApplication1.Models
{
    public class BorrowDetail
    {
        public int Id { get; set; }

        public int BorrowRecordId { get; set; }

        public int BookId { get; set; }

        public int Quantity { get; set; }

        public BorrowRecord? BorrowRecord { get; set; }

        public Book? Book { get; set; }
    }
}