using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);
// 1. Đọc thông tin JwtSettings từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

// 2. Cấu hình dịch vụ Authentication với JwtBearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = ClaimTypes.Role
    };
});

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
// 2. Bổ sung đoạn này để hiện nút Authorize trên Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectAPIdemo", Version = "v1" });

    // Định nghĩa khung nhập Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập trực tiếp chuỗi Token thu được sau khi Login."
    });

    // Áp dụng khung nhập Token cho toàn bộ API
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});


// Đăng ký Dependency Injection
builder.Services.AddScoped<WebApplication1.Repositories.IAuthRepository, WebApplication1.Repositories.AuthRepository>();
builder.Services.AddScoped<WebApplication1.Services.IAuthService, WebApplication1.Services.AuthService>();
var app = builder.Build();

// Bật Swagger trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tự động Seed tài khoản Admin và dữ liệu ban đầu
await DbInitializer.SeedAsync(app.Services);


app.Run();