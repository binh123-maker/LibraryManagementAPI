using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. THỐNG KÊ TỔNG QUAN HỆ THỐNG THƯ VIỆN
        // GET: api/dashboard/stats
        // =====================================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            var today = DateTime.UtcNow.Date;

            int totalBookTitles = await _context.Books.CountAsync();
            int totalBookCopies = await _context.Books.SumAsync(b => (int?)b.Quantity) ?? 0;
            int availableBookCopies = await _context.Books.SumAsync(b => (int?)b.AvailableQuantity) ?? 0;
            int borrowingCopies = totalBookCopies - availableBookCopies;

            int totalCategories = await _context.Categories.CountAsync();
            int totalUsers = await _context.Users.CountAsync();
            int totalStudents = await _context.Users.CountAsync(u => u.Role == "Reader");

            int totalBorrowRecords = await _context.BorrowRecords.CountAsync();
            int activeBorrowingRecords = await _context.BorrowRecords.CountAsync(br => br.ReturnDate == null);
            int overdueRecords = await _context.BorrowRecords.CountAsync(br => br.ReturnDate == null && br.DueDate.Date < today);

            decimal totalFines = await _context.BorrowRecords.SumAsync(br => (decimal?)br.FineAmount) ?? 0;

            return Ok(new
            {
                books = new
                {
                    totalTitles = totalBookTitles,
                    totalCopies = totalBookCopies,
                    availableCopies = availableBookCopies,
                    borrowingCopies = borrowingCopies
                },
                categories = totalCategories,
                users = new
                {
                    totalUsers = totalUsers,
                    students = totalStudents,
                    staff = totalUsers - totalStudents
                },
                borrowing = new
                {
                    totalRecords = totalBorrowRecords,
                    activeRecords = activeBorrowingRecords,
                    overdueRecords = overdueRecords,
                    totalFineAmount = totalFines
                }
            });
        }

        // =====================================================
        // 2. TOP SÁCH ĐƯỢC MƯỢN NHIỀU NHẤT
        // GET: api/dashboard/top-books?limit=5
        // =====================================================
        [HttpGet("top-books")]
        public async Task<IActionResult> GetTopBorrowedBooks([FromQuery] int limit = 5)
        {
            var topBooks = await _context.BorrowDetails
                .GroupBy(bd => new { bd.BookId, bd.Book!.Title, bd.Book.Author, bd.Book.Category!.Name })
                .Select(g => new
                {
                    bookId = g.Key.BookId,
                    title = g.Key.Title,
                    author = g.Key.Author,
                    categoryName = g.Key.Name,
                    totalBorrowCount = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.totalBorrowCount)
                .Take(limit)
                .ToListAsync();

            return Ok(topBooks);
        }

        // =====================================================
        // 3. DANH SÁCH SINH VIÊN QUÁ HẠN ĐANG NỢ SÁCH
        // GET: api/dashboard/overdue-students
        // =====================================================
        [HttpGet("overdue-students")]
        public async Task<IActionResult> GetOverdueStudents()
        {
            var today = DateTime.UtcNow.Date;

            var overdueList = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(br => br.ReturnDate == null && br.DueDate.Date < today)
                .OrderBy(br => br.DueDate)
                .Select(br => new
                {
                    borrowRecordId = br.Id,
                    userId = br.UserId,
                    fullName = br.User != null ? br.User.FullName : string.Empty,
                    studentCode = br.User != null ? br.User.StudentCode : null,
                    department = br.User != null ? br.User.Department : null,
                    classRoom = br.User != null ? br.User.ClassRoom : null,
                    email = br.User != null ? br.User.Email : string.Empty,
                    phoneNumber = br.User != null ? br.User.PhoneNumber : null,
                    borrowDate = br.BorrowDate,
                    dueDate = br.DueDate,
                    overdueDays = (today - br.DueDate.Date).Days,
                    estimatedFine = (today - br.DueDate.Date).Days * 5000,
                    borrowedBooks = br.BorrowDetails.Select(bd => new
                    {
                        bookId = bd.BookId,
                        title = bd.Book != null ? bd.Book.Title : string.Empty,
                        quantity = bd.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(overdueList);
        }
    }
}
