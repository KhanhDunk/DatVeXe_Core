using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.DTO;
using Models.Models;
using Service.Background;
using Service.Interface;
using Service.Service;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// JWT config
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new Exception("JWT Key not found"));
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];





builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddDbContextPool<BookingSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Controllers
builder.Services.AddControllers();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();


builder.Services.Configure<EmailSetting>(
    builder.Configuration.GetSection("EmailSettings"));

// Register EmailSender so IEmailSender can be resolved by controllers
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IEmailBackgroundQueue, EmailBackgroundQueue>();
builder.Services.AddHostedService<QueuedEmailSender>();



// Rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Đăng ký rate limit nếu request vượt quá limit trả về HTTP 429 Too Many Requests 

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            code = "429",
            message = "Bạn thao tác quá nhanh. Vui lòng thử lại sau."
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };

    // -------- REGISTER (limit theo IP) --------
    options.AddPolicy("register_limit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip"; // Lấy IP của client

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions //FixedWindowRateLimiter  giới hạn x request trong y phút 
            {
                PermitLimit = 5,// Tối đa 5 request 
                Window = TimeSpan.FromMinutes(1), // Trong vòng 1 phút 
                QueueLimit = 0
            });
    });

    // -------- LOGIN (limit theo IP) --------
    options.AddPolicy("login_limit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: ip,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 10, // Tối đa 10 lần 
                TokensPerPeriod = 10, // Hồi 10 token 
                ReplenishmentPeriod = TimeSpan.FromMinutes(1), // Mỗi 1 phút 
                AutoReplenishment = true, // Tự hồi token 
                QueueLimit = 0 // Không chờ 
            });
    });

    // -------- FORGOT PASSWORD --------
    options.AddPolicy("forgot_password_limit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: ip,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 1,
                TokensPerPeriod = 1,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueLimit = 0
            });
    });
});

// --- Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Đặt vé xe API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token (chỉ dán token, không cần Bearer)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});




builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins(
                "http://localhost:4200",
                "http://localhost:4300"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});



// Build app
var app = builder.Build();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Đặt vé xe API V1");
        c.RoutePrefix = "";
    });
}



//app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();