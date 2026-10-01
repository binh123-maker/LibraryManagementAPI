using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<BorrowDetail> BorrowDetails { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc Unique cho Username và Email trong bảng Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình độ chính xác cho giá sách
            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)");

            // Cấu hình độ chính xác cho tiền phạt
            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.FineAmount)
                .HasColumnType("decimal(18,2)");

            // Quan hệ 1 Category - Nhiều Book (Xóa danh mục không được làm mất sách bừa bãi)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1 User - Nhiều BorrowRecord
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1 BorrowRecord - Nhiều BorrowDetail (Xóa phiếu mượn sẽ xóa các dòng chi tiết)
            modelBuilder.Entity<BorrowDetail>()
                .HasOne(bd => bd.BorrowRecord)
                .WithMany(br => br.BorrowDetails)
                .HasForeignKey(bd => bd.BorrowRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1 Book - Nhiều BorrowDetail (Không cho xóa sách nếu sách đang có trong phiếu mượn)
            modelBuilder.Entity<BorrowDetail>()
                .HasOne(bd => bd.Book)
                .WithMany(b => b.BorrowDetails)
                .HasForeignKey(bd => bd.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1 User - Nhiều Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}