using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            AppDbContext context,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Email người nhận đang để trống.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) || string.IsNullOrWhiteSpace(_emailSettings.SenderPassword))
            {
                _logger.LogWarning("Chưa cấu hình tài khoản Gmail Sender trong appsettings.json. Bỏ qua việc gửi mail thực tế.");
                return false;
            }

            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Gửi email thành công đến {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đến {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
        }

        public async Task<int> SendDueDateRemindersAsync(int daysBeforeDue = 2)
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeDue);

            // Tìm các phiếu mượn chưa trả và có hạn trả trong khoảng sắp tới
            var records = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(br => br.ReturnDate == null && br.DueDate.Date == targetDate)
                .ToListAsync();

            int sentCount = 0;

            foreach (var record in records)
            {
                if (record.User == null || string.IsNullOrWhiteSpace(record.User.Email))
                {
                    continue;
                }

                var bookListHtml = string.Join("", record.BorrowDetails.Select(bd =>
                    $"<li><strong>{bd.Book?.Title ?? "Tài liệu"}</strong> - Số lượng: {bd.Quantity}</li>"));

                var subject = $"[Thư viện] Nhắc nhở: Sách mượn sắp đến hạn trả ({record.DueDate:dd/MM/yyyy})";
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px;'>
                        <h2 style='color: #1976d2; border-bottom: 2px solid #1976d2; padding-bottom: 10px;'>THƯ VIỆN ĐẠI HỌC - THÔNG BÁO NHẮC TRẢ SÁCH</h2>
                        <p>Xin chào <strong>{record.User.FullName}</strong> {(string.IsNullOrEmpty(record.User.StudentCode) ? "" : $"(MSSV: {record.User.StudentCode})")},</p>
                        <p>Hệ thống thư viện xin thông báo phiếu mượn <strong>#{record.Id}</strong> của bạn sắp đến hạn hoàn trả:</p>
                        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 6px; margin: 15px 0;'>
                            <p><strong>Ngày mượn:</strong> {record.BorrowDate:dd/MM/yyyy}</p>
                            <p><strong>Hạn chót trả sách:</strong> <span style='color: #d32f2f; font-weight: bold;'>{record.DueDate:dd/MM/yyyy}</span></p>
                            <p><strong>Danh sách sách đang mượn:</strong></p>
                            <ul>
                                {bookListHtml}
                            </ul>
                        </div>
                        <p>Vui lòng mang sách đến quầy thư viện trước hoặc đúng ngày <strong>{record.DueDate:dd/MM/yyyy}</strong> để tránh bị phát sinh phí phạt quá hạn (5.000đ/ngày).</p>
                        <p style='color: #777; font-size: 13px; margin-top: 30px;'>Đây là email tự động từ hệ thống Quản lý Thư viện. Vui lòng không trả lời thư này.</p>
                    </div>";

                bool isSuccess = await SendEmailAsync(record.User.Email, subject, htmlBody);

                // Lưu lại thông báo In-App vào database
                var notification = new Notification
                {
                    UserId = record.UserId,
                    Title = "Nhắc nhở hạn trả sách",
                    Message = $"Phiếu mượn #{record.Id} sắp hết hạn vào ngày {record.DueDate:dd/MM/yyyy}. Vui lòng mang sách đến hoàn trả đúng hạn.",
                    Type = "DueReminder",
                    IsSentViaEmail = isSuccess,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                sentCount++;
            }

            await _context.SaveChangesAsync();
            return sentCount;
        }

        public async Task<int> SendOverdueAlertsAsync()
        {
            var now = DateTime.UtcNow.Date;

            // Tìm các phiếu mượn chưa trả và đã quá hạn
            var records = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.Book)
                .Where(br => br.ReturnDate == null && br.DueDate.Date < now)
                .ToListAsync();

            int sentCount = 0;

            foreach (var record in records)
            {
                if (record.User == null || string.IsNullOrWhiteSpace(record.User.Email))
                {
                    continue;
                }

                int overdueDays = (now - record.DueDate.Date).Days;
                decimal estimatedFine = overdueDays * 5000m; // 5.000 VNĐ / ngày

                // Cập nhật trạng thái phiếu mượn thành Overdue nếu chưa set
                if (record.Status != "Lost")
                {
                    record.Status = "Overdue";
                    record.FineAmount = estimatedFine;
                }

                var bookListHtml = string.Join("", record.BorrowDetails.Select(bd =>
                    $"<li><strong>{bd.Book?.Title ?? "Tài liệu"}</strong> - Số lượng: {bd.Quantity}</li>"));

                var subject = $"[CẢNH BÁO] Phiếu mượn sách đã quá hạn {overdueDays} ngày (Mã #{record.Id})";
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #ffcdd2; border-radius: 8px; padding: 20px;'>
                        <h2 style='color: #c62828; border-bottom: 2px solid #c62828; padding-bottom: 10px;'>CẢNH BÁO MƯỢN SÁCH QUÁ HẠN</h2>
                        <p>Kính gửi sinh viên: <strong>{record.User.FullName}</strong> {(string.IsNullOrEmpty(record.User.StudentCode) ? "" : $"(MSSV: {record.User.StudentCode})")},</p>
                        <p>Hệ thống ghi nhận bạn đang giữ sách <span style='color: #c62828; font-weight: bold;'>QUÁ HẠN {overdueDays} NGÀY</span> đối với phiếu mượn <strong>#{record.Id}</strong>.</p>
                        <div style='background-color: #ffebee; padding: 15px; border-radius: 6px; margin: 15px 0;'>
                            <p><strong>Hạn trả quy định:</strong> {record.DueDate:dd/MM/yyyy}</p>
                            <p><strong>Số ngày quá hạn:</strong> <span style='color: #c62828; font-weight: bold;'>{overdueDays} ngày</span></p>
                            <p><strong>Ước tính tiền phạt trễ hạn:</strong> <span style='color: #c62828; font-weight: bold;'>{estimatedFine:N0} VNĐ</span></p>
                            <p><strong>Danh sách sách đang giữ:</strong></p>
                            <ul>
                                {bookListHtml}
                            </ul>
                        </div>
                        <p style='color: #d32f2f; font-weight: bold;'>Vui lòng đến ngay quầy thư viện để hoàn trả tài liệu và thanh toán tiền phạt.</p>
                        <p>Nếu không hoàn trả, tài khoản của bạn sẽ bị khóa và thông tin sẽ được chuyển sang Phòng Đào tạo / CTSV để xử lý theo quy chế trường.</p>
                    </div>";

                bool isSuccess = await SendEmailAsync(record.User.Email, subject, htmlBody);

                var notification = new Notification
                {
                    UserId = record.UserId,
                    Title = "Cảnh báo quá hạn mượn sách",
                    Message = $"Phiếu mượn #{record.Id} đã quá hạn {overdueDays} ngày. Số tiền phạt dự tính: {estimatedFine:N0} VNĐ.",
                    Type = "OverdueWarning",
                    IsSentViaEmail = isSuccess,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                sentCount++;
            }

            await _context.SaveChangesAsync();
            return sentCount;
        }
    }
}
