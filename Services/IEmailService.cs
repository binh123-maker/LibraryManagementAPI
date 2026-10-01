namespace WebApplication1.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi 1 email bất kỳ với nội dung HTML
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);

        /// <summary>
        /// Tự động quét các phiếu mượn sắp đến hạn (mặc định trước 2 ngày) và gửi Gmail nhắc nhở
        /// </summary>
        Task<int> SendDueDateRemindersAsync(int daysBeforeDue = 2);

        /// <summary>
        /// Tự động quét các phiếu mượn đã quá hạn và gửi Gmail cảnh báo kèm số tiền phạt
        /// </summary>
        Task<int> SendOverdueAlertsAsync();
    }
}
