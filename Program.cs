using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Controller
builder.Services.AddControllers();

// Kết nối SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Cấu hình EmailSettings
builder.Services.Configure<WebApplication1.Models.EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

// Đăng ký EmailService
builder.Services.AddScoped<WebApplication1.Services.IEmailService, WebApplication1.Services.EmailService>();

// Đăng ký Repository & Service (User)
builder.Services.AddScoped<WebApplication1.Repositories.IUserRepository, WebApplication1.Repositories.UserRepository>();
builder.Services.AddScoped<WebApplication1.Services.IUserService, WebApplication1.Services.UserService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Bật Swagger trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Tự động Seed tài khoản Admin và dữ liệu ban đầu
await DbInitializer.SeedAsync(app.Services);

app.Run();