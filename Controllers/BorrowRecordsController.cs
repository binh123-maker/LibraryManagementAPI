using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BorrowRecordsController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. GET ALL BORROW RECORDS (Hỗ trợ lọc theo trạng thái & từ khóa)
        // GET: api/borrowrecords?status=Borrowing&keyword=Nguyen
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponseDto>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? keyword)
        {
            var query = _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(br => br.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(br =>
                    (br.User != null && (br.User.FullName.Contains(keyword) ||
                                         (br.User.StudentCode != null && br.User.StudentCode.Contains(keyword)) ||
                                         br.User.Username.Contains(keyword))));
            }

            var records = await query
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordResponseDto
                {
                    Id = br.Id,
                    UserId = br.UserId,
                    UserName = br.User != null ? br.User.Username : string.Empty,
                    FullName = br.User != null ? br.User.FullName : string.Empty,
                    StudentCode = br.User != null ? br.User.StudentCode : null,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status,
                    FineAmount = br.FineAmount,
                    Note = br.Note,
                    Details = br.BorrowDetails.Select(bd => new BorrowDetailResponseDto
                    {
                        Id = bd.Id,
                        BookId = bd.BookId,
                        BookTitle = bd.Book != null ? bd.Book.Title : string.Empty,
                        BookPrice = bd.Book != null ? bd.Book.Price : 0,
                        Quantity = bd.Quantity,
                        IsReturned = bd.IsReturned,
                        ReturnedDate = bd.ReturnedDate,
                        BookCondition = bd.BookCondition,
                        Note = bd.Note
                    }).ToList()
                })
                .ToListAsync();

            return Ok(records);
        }

        // =====================================================
        // 2. GET BY ID
        // GET: api/borrowrecords/1
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BorrowRecordResponseDto>> GetById(int id)
        {
            var record = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (record == null)
            {
                return NotFound(new { message = $"Không tìm thấy phiếu mượn có Id = {id}" });
            }

            var response = new BorrowRecordResponseDto
            {
                Id = record.Id,
                UserId = record.UserId,
                UserName = record.User?.Username ?? string.Empty,
                FullName = record.User?.FullName ?? string.Empty,
                StudentCode = record.User?.StudentCode,
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status,
                FineAmount = record.FineAmount,
                Note = record.Note,
                Details = record.BorrowDetails.Select(bd => new BorrowDetailResponseDto
                {
                    Id = bd.Id,
                    BookId = bd.BookId,
                    BookTitle = bd.Book?.Title ?? string.Empty,
                    BookPrice = bd.Book?.Price ?? 0,
                    Quantity = bd.Quantity,
                    IsReturned = bd.IsReturned,
                    ReturnedDate = bd.ReturnedDate,
                    BookCondition = bd.BookCondition,
                    Note = bd.Note
                }).ToList()
            };

            return Ok(response);
        }

        // =====================================================
        // 3. GET BY USER ID
        // GET: api/borrowrecords/user/1
        // =====================================================
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponseDto>>> GetByUserId(int userId)
        {
            var records = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(br => br.UserId == userId)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordResponseDto
                {
                    Id = br.Id,
                    UserId = br.UserId,
                    UserName = br.User != null ? br.User.Username : string.Empty,
                    FullName = br.User != null ? br.User.FullName : string.Empty,
                    StudentCode = br.User != null ? br.User.StudentCode : null,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status,
                    FineAmount = br.FineAmount,
                    Note = br.Note,
                    Details = br.BorrowDetails.Select(bd => new BorrowDetailResponseDto
                    {
                        Id = bd.Id,
                        BookId = bd.BookId,
                        BookTitle = bd.Book != null ? bd.Book.Title : string.Empty,
                        BookPrice = bd.Book != null ? bd.Book.Price : 0,
                        Quantity = bd.Quantity,
                        IsReturned = bd.IsReturned,
                        ReturnedDate = bd.ReturnedDate,
                        BookCondition = bd.BookCondition,
                        Note = bd.Note
                    }).ToList()
                })
                .ToListAsync();

            return Ok(records);
        }

        // =====================================================
        // 4. CREATE BORROW RECORD (Tạo phiếu mượn sách)
        // POST: api/borrowrecords
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<BorrowRecordResponseDto>> Create([FromBody] CreateBorrowRecordDto dto)
        {
            // Kiểm tra người mượn
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Không tìm thấy người dùng trong hệ thống." });
            }

            if (!user.IsActive)
            {
                return BadRequest(new { message = "Tài khoản người dùng này đang bị khóa, không được phép mượn sách." });
            }

            // Kiểm tra xem độc giả có đang nợ sách quá hạn chưa trả không
            bool hasOverdueRecords = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == dto.UserId && br.ReturnDate == null && br.DueDate < DateTime.UtcNow);

            if (hasOverdueRecords)
            {
                return BadRequest(new { message = "Sinh viên/Độc giả đang có sách quá hạn chưa trả. Vui lòng hoàn trả sách trước khi mượn mới!" });
            }

            // Kiểm tra và trừ số lượng từng cuốn sách trong kho
            var bookIds = dto.Items.Select(i => i.BookId).Distinct().ToList();
            var books = await _context.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();

            if (books.Count != bookIds.Count)
            {
                return BadRequest(new { message = "Có một hoặc nhiều cuốn sách không tồn tại trong hệ thống." });
            }

            var borrowRecord = new BorrowRecord
            {
                UserId = dto.UserId,
                BorrowDate = DateTime.UtcNow,
                DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(14), // Mặc định 14 ngày
                Status = "Borrowing",
                FineAmount = 0,
                Note = dto.Note
            };

            foreach (var item in dto.Items)
            {
                var book = books.First(b => b.Id == item.BookId);

                if (book.AvailableQuantity < item.Quantity)
                {
                    return BadRequest(new
                    {
                        message = $"Cuốn sách '{book.Title}' chỉ còn {book.AvailableQuantity} cuốn trong kho, không đủ số lượng mượn ({item.Quantity})."
                    });
                }

                // Trừ số lượng có sẵn trên kệ
                book.AvailableQuantity -= item.Quantity;

                borrowRecord.BorrowDetails.Add(new BorrowDetail
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    IsReturned = false,
                    BookCondition = "Good"
                });
            }

            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = borrowRecord.Id }, new { message = "Tạo phiếu mượn sách thành công!", recordId = borrowRecord.Id });
        }

        // =====================================================
        // 5. RETURN BOOKS (Xử lý trả sách, tính phí phạt & cập nhật kho)
        // PUT: api/borrowrecords/1/return
        // =====================================================
        [HttpPut("{id:int}/return")]
        public async Task<IActionResult> ReturnBooks(int id, [FromBody] ReturnBorrowRecordDto dto)
        {
            var record = await _context.BorrowRecords
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (record == null)
            {
                return NotFound(new { message = $"Không tìm thấy phiếu mượn có Id = {id}" });
            }

            if (record.Status == "Returned")
            {
                return BadRequest(new { message = "Phiếu mượn này đã được hoàn trả trước đó." });
            }

            var now = DateTime.UtcNow;
            record.ReturnDate = now;

            decimal totalFine = 0;

            // Tính phạt trễ hạn nếu có
            if (now.Date > record.DueDate.Date)
            {
                int overdueDays = (now.Date - record.DueDate.Date).Days;
                totalFine += overdueDays * 5000m; // 5.000 VNĐ / ngày
            }

            // Xử lý từng cuốn sách trong phiếu
            foreach (var detail in record.BorrowDetails)
            {
                // Tìm thông tin trả trong request nếu có chỉ định riêng
                var returnInfo = dto.Items.FirstOrDefault(i => i.BorrowDetailId == detail.Id);
                string condition = returnInfo?.Condition ?? "Good";
                string? note = returnInfo?.Note;

                detail.IsReturned = true;
                detail.ReturnedDate = now;
                detail.BookCondition = condition;
                if (!string.IsNullOrEmpty(note)) detail.Note = note;

                if (detail.Book != null)
                {
                    if (condition == "Lost")
                    {
                        // Nếu mất sách: Đền 100% giá sách và KHÔNG cộng lại vào kho
                        totalFine += detail.Book.Price * detail.Quantity;
                    }
                    else if (condition == "Damaged")
                    {
                        // Nếu sách bị hư hại: Phạt 50.000đ tiền sửa chữa/bồi thường và cộng lại vào kho
                        totalFine += 50000m * detail.Quantity;
                        detail.Book.AvailableQuantity += detail.Quantity;
                    }
                    else
                    {
                        // Sách bình thường: Trả lại kho
                        detail.Book.AvailableQuantity += detail.Quantity;
                    }
                }
            }

            record.FineAmount = totalFine;
            record.Status = record.BorrowDetails.Any(bd => bd.BookCondition == "Lost") ? "Lost" : "Returned";
            if (!string.IsNullOrEmpty(dto.Note)) record.Note = dto.Note;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xử lý trả sách thành công!",
                recordId = record.Id,
                status = record.Status,
                fineAmount = record.FineAmount,
                returnDate = record.ReturnDate
            });
        }

        // =====================================================
        // 6. EXTEND DUE DATE (Gia hạn phiếu mượn)
        // PUT: api/borrowrecords/1/extend
        // =====================================================
        [HttpPut("{id:int}/extend")]
        public async Task<IActionResult> ExtendDueDate(int id, [FromBody] ExtendBorrowRecordDto dto)
        {
            var record = await _context.BorrowRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Không tìm thấy phiếu mượn có Id = {id}" });
            }

            if (record.Status == "Returned")
            {
                return BadRequest(new { message = "Không thể gia hạn phiếu mượn đã hoàn trả." });
            }

            if (DateTime.UtcNow.Date > record.DueDate.Date)
            {
                return BadRequest(new { message = "Phiếu mượn đã quá hạn. Vui lòng hoàn trả sách và thanh toán tiền phạt, không thể gia hạn!" });
            }

            record.DueDate = record.DueDate.AddDays(dto.ExtraDays);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Gia hạn thành công thêm {dto.ExtraDays} ngày.",
                newDueDate = record.DueDate
            });
        }
    }
}
