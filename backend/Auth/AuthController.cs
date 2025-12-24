using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.DTO;
using Models.Models;
using Service.Background;
using Service.Interface;
using Service.Service;
using Service.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Auth
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly JwtService _jwtService;
        private IUserService _userService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailBackgroundQueue _emailQueue;
        private readonly BookingSystemContext _context;

        public AuthController(JwtService jwtService, IUserService userService, IPasswordHasher<User> passwordHasher, BookingSystemContext context, IEmailBackgroundQueue emailQueue)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _context = context;
            _emailQueue = emailQueue;
        }

        [HttpPost("login")]
        public ActionResult<ResponseDTO<string>> Login([FromBody] LoginDTO login)
        {
            try
            {
                if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
                {
                    return BadRequest(new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Username và password không được để trống",
                        Data = null
                    });
                }

                var user = _userService.GetByUsername(login.Username);
                if (user == null)
                {
                    return Unauthorized(new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Username hoặc password không đúng",
                        Data = null
                    });
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, login.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Username hoặc password không đúng",
                        Data = null
                    });
                }

                var token = _jwtService.GenerateToken(user);

                return Ok(new ResponseDTO<string>
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"[Login] Exception: {ex.Message}");

                return StatusCode(500, new ResponseDTO<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập: " + ex.Message,
                    Data = null
                });
            }
        }


        [HttpPost("register")]
        public ActionResult<ResponseDTO<object>> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            if (_userService.Exists(dto.Username, dto.Email))
            {
                return BadRequest(new ResponseDTO<object>
                {
                    Success = false,
                    Message = "Username hoặc Email đã tồn tại"
                });
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Phone = dto.Phone
            };

            user.Password = _passwordHasher.HashPassword(user, dto.Password);

            _userService.create(user);

            return Ok(new ResponseDTO<object>
            {
                Success = true,
                Message = "Đăng ký thành công. Vui lòng đăng nhập để tiếp tục."
            });
        }

        // 1️⃣ Quên mật khẩu: gửi OTP
        [HttpPost("forgot-password")]
        [EnableRateLimiting("forgot_password_limit")]
        public async Task<ActionResult<ResponseDTO<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ResponseDTO<string>(false, "Email không hợp lệ", null, "INVALID_INPUT"));
            }

            var normalizedEmail = request.Email.Trim();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                return BadRequest(new ResponseDTO<string>(false, "Email không tồn tại", null, "EMAIL_NOT_FOUND"));
            }

            var otp = Helper.GenerateOtp();
            var otpHash = Helper.HashOtp(otp);

            var existingToken = await _context.OtpTokens
                .FirstOrDefaultAsync(o =>
                    o.UserId == user.UserId &&
                    o.OtpType == "reset_password" &&
                    o.IsUsed == false);

            if (existingToken != null)
            {
                existingToken.OtpCodeHash = otpHash;
                existingToken.Email = user.Email;
                existingToken.AttemptCount = 0;
                existingToken.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
                existingToken.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.OtpTokens.Add(new OtpToken
                {
                    Email = user.Email,
                    UserId = user.UserId,
                    OtpCodeHash = otpHash,
                    OtpType = "reset_password",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            await _emailQueue.QueueEmailAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "Mã OTP Reset Password",
                Body = $"Mã OTP của bạn là: {otp}",
                IsHtml = false
            });

            return Ok(new ResponseDTO<string>(true, "Đã gửi mã OTP về email", null, "OTP_SENT"));
        }

        // 2️⃣ Reset Password bằng OTP
        [HttpPost("reset-password")]
        public async Task<ActionResult<ResponseDTO<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Otp) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new ResponseDTO<string>(false, "Dữ liệu không hợp lệ", null, "INVALID_INPUT"));
            }

            var normalizedEmail = request.Email.Trim();
            var otpCode = request.Otp.Trim();

            var otp = await _context.OtpTokens
                .AsTracking()
                .Include(o => o.User)
                .Where(x =>
                    x.Email == normalizedEmail &&
                    x.OtpType == "reset_password" &&
                    x.IsUsed != true &&
                    x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
            {
                return BadRequest(new ResponseDTO<string>(false, "OTP không hợp lệ hoặc đã hết hạn", null, "OTP_INVALID"));
            }

            if (otp.AttemptCount >= otp.MaxAttempt)
            {
                return BadRequest(new ResponseDTO<string>(false, "OTP đã bị khóa", null, "OTP_LOCKED"));
            }

            if (!Helper.VerifyOtp(otpCode, otp.OtpCodeHash))
            {
                otp.AttemptCount++;
                await _context.SaveChangesAsync();
                return BadRequest(new ResponseDTO<string>(false, "OTP sai", null, "OTP_INCORRECT"));
            }

            if (otp.User == null)
            {
                otp.User = await _context.Users.FindAsync(otp.UserId);
                if (otp.User == null)
                {
                    return BadRequest(new ResponseDTO<string>(false, "Người dùng không tồn tại", null, "USER_NOT_FOUND"));
                }
            }

            otp.User.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ResponseDTO<string>(true, "Đổi mật khẩu thành công", null, "PASSWORD_RESET_SUCCESS"));
        }

    }

}

