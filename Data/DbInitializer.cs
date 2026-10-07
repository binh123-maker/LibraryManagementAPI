using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Đảm bảo Database đã được tạo
            await context.Database.MigrateAsync();

            // 1. Seed tài khoản Admin mặc định
            if (!await context.Users.AnyAsync(u => u.Role == "Admin"))
            {
                using var hmac = new HMACSHA512();
                var passwordSalt = hmac.Key;
                var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Admin@123"));

                var admin = new User
                {
                    Username = "admin",
                    FullName = "Quản Trị Viên Hệ Thống",
                    Email = "admin@library.edu.vn",
                    PhoneNumber = "0901234567",
                    Address = "Phòng Thư viện - Tầng 2",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
            }

            // 2. Seed Danh mục mẫu nếu chưa có
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Công nghệ thông tin", Description = "Sách giáo trình và tài liệu lập trình, mạng máy tính, AI" },
                    new Category { Name = "Kinh tế & Quản trị", Description = "Sách tài chính, kế toán, marketing, quản trị kinh doanh" },
                    new Category { Name = "Khoa học & Kỹ thuật", Description = "Sách vật lý, toán học, cơ điện tử, tự động hóa" },
                    new Category { Name = "Kỹ năng mềm & Ngoại ngữ", Description = "Tài liệu tiếng Anh, kỹ năng giao tiếp và thuyết trình" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 3. Seed Sách mẫu nếu chưa có
            if (!await context.Books.AnyAsync())
            {
                var itCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Công nghệ thông tin");
                int catId = itCategory?.Id ?? 1;

                var books = new List<Book>
                {
                    new Book
                    {
                        Title = "Lập trình C# & .NET Core nâng cao",
                        Author = "Nguyễn Văn A",
                        Publisher = "NXB Giáo Dục",
                        PublishYear = 2024,
                        Quantity = 10,
                        AvailableQuantity = 10,
                        Price = 120000m,
                        Location = "Kệ A1-01",
                        Description = "Giáo trình toàn diện về lập trình C# hiện đại và xây dựng Web API",
                        CategoryId = catId
                    },
                    new Book
                    {
                        Title = "Cấu trúc dữ liệu và giải thuật",
                        Author = "Trần Đình B",
                        Publisher = "NXB Đại Học Quốc Gia",
                        PublishYear = 2023,
                        Quantity = 8,
                        AvailableQuantity = 8,
                        Price = 95000m,
                        Location = "Kệ A1-02",
                        Description = "Kiến thức nền tảng về thuật toán và tối ưu hóa mã nguồn",
                        CategoryId = catId
                    },
                    new Book
                    {
                        Title = "Thiết kế Cơ sở dữ liệu và SQL Server",
                        Author = "Lê Hoàng C",
                        Publisher = "NXB Thông Tin & Truyền Thông",
                        PublishYear = 2023,
                        Quantity = 15,
                        AvailableQuantity = 15,
                        Price = 110000m,
                        Location = "Kệ A2-01",
                        Description = "Thực hành thiết kế CSDL quan hệ, chuẩn hóa dữ liệu và viết truy vấn",
                        CategoryId = catId
                    }
                };

                context.Books.AddRange(books);
            }

            await context.SaveChangesAsync();
        }
    }
}
