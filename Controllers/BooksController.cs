using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/books
        // GET: api/books?title=lap+trinh
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] string? title)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(b => b.Title.Contains(title));
            }

            return await query.ToListAsync();
        }

        // POST: api/books
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(BookDto dto)
        {
            var categoryExists =
                await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Thể loại không tồn tại.");
            }

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                Publisher = dto.Publisher,
                PublishYear = dto.PublishYear,
                Quantity = dto.Quantity,
                AvailableQuantity = dto.AvailableQuantity ?? dto.Quantity,
                ImageUrl = dto.ImageUrl,
                Location = dto.Location,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBooks),
                new { id = book.Id },
                book
            );
        }

        // PUT: api/books/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDto dto)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            var categoryExists =
                await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Thể loại không tồn tại.");
            }

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Publisher = dto.Publisher;
            book.PublishYear = dto.PublishYear;
            book.Quantity = dto.Quantity;
            if (dto.AvailableQuantity.HasValue)
            {
                book.AvailableQuantity = dto.AvailableQuantity.Value;
            }
            book.ImageUrl = dto.ImageUrl;
            book.Location = dto.Location;
            book.Description = dto.Description;
            book.Price = dto.Price;
            book.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/books/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/books/search?keyword=java
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
            string keyword)
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Where(b =>
                    b.Title.Contains(keyword) ||
                    b.Author.Contains(keyword) ||
                    (b.ISBN != null && b.ISBN.Contains(keyword)))
                .ToListAsync();

            return books;
        }
    }
}