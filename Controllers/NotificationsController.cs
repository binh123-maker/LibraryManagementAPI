using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public NotificationsController(IEmailService emailService, AppDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        // =====================================================
        // 1. TEST GỬI GMAIL
        // POST: api/notifications/test-email?toEmail=abc@gmail.com
        // =====================================================
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string toEmail)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return BadRequest(new { message = "Email nhận không được để trống." });
            }

            var subject = "[Thư viện] Email kiểm tra kết nối hệ thống";
            var body = "<div style='font-family: Arial; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>" +
                       "<h3 style='color: #2e7d32;'>Kết nối Gmail thành công!</h3>" +
                       "<p>Hệ thống Quản lý Thư viện trường Đại học đã cấu hình thành công dịch vụ gửi mail qua Google SMTP.</p>" +
                       "</div>";

            bool result = await _emailService.SendEmailAsync(toEmail, subject, body);

            if (result)
            {
                return Ok(new { message = $"Đã gửi email thử nghiệm thành công tới {toEmail}." });
            }

            return BadRequest(new { message = "Gửi email thất bại. Vui lòng kiểm tra lại cấu hình EmailSettings trong appsettings.json." });
        }

        // =====================================================
        // 2. KÍCH HOẠT QUÉT VÀ GỬI EMAIL NHẮC HẠN SẮP TỚI
        // POST: api/notifications/send-due-reminders?daysBeforeDue=2
        // =====================================================
        [HttpPost("send-due-reminders")]
        public async Task<IActionResult> TriggerDueDateReminders([FromQuery] int daysBeforeDue = 2)
        {
            int sentCount = await _emailService.SendDueDateRemindersAsync(daysBeforeDue);

            return Ok(new
            {
                message = $"Đã quét và xử lý nhắc nhở hạn trả sách thành công.",
                remindersSent = sentCount
            });
        }

        // =====================================================
        // 3. KÍCH HOẠT QUÉT VÀ GỬI EMAIL CẢNH BÁO QUÁ HẠN
        // POST: api/notifications/send-overdue-alerts
        // =====================================================
        [HttpPost("send-overdue-alerts")]
        public async Task<IActionResult> TriggerOverdueAlerts()
        {
            int sentCount = await _emailService.SendOverdueAlertsAsync();

            return Ok(new
            {
                message = "Đã quét và xử lý cảnh báo sách quá hạn thành công.",
                alertsSent = sentCount
            });
        }

        // =====================================================
        // 4. LẤY DANH SÁCH THÔNG BÁO CỦA SINH VIÊN (IN-APP)
        // GET: api/notifications/user/{userId}
        // =====================================================
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // =====================================================
        // 5. ĐÁNH DẤU THÔNG BÁO ĐÃ ĐỌC
        // PUT: api/notifications/{id}/mark-as-read
        // =====================================================
        [HttpPut("{id:int}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Không tìm thấy thông báo." });
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
        }
    }
}
