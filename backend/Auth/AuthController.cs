using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models.DTO;
using Models.Models;
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

        public AuthController(JwtService jwtService, IUserService userService, IPasswordHasher<User> passwordHasher)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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


    }

}

