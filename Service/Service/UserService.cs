using Microsoft.EntityFrameworkCore;
using Models.Models;
using Service.Interface;
using Service.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class UserService : IUserService
    {
        private readonly BookingSystemContext _context;

        public UserService(BookingSystemContext context)
        {
            _context = context;
        }


        public User? GetByUsername(string username)
        {
            return _context.Users
                .Include(u => u.Role) // ⭐ BẮT BUỘC
                .FirstOrDefault(u => u.Username == username);
        }


        public bool Exists(string username, string email)
        {
            return _context.Users.Any(u => u.Username == username || u.Email == email);
        }

        public void create(User user)
        {
            // Lấy role mặc định "User" từ database
            var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (defaultRole == null)
            {
                throw new Exception("Role 'User' không tồn tại trong database.");
            }

            // Gán role mặc định
            user.RoleId = defaultRole.RoleId; // hoặc user.Role = defaultRole;
            user.Role = defaultRole;
            // Thêm user vào database
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public List<UserDTO> GetAll()
        {
            return _context.Users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Phone = u.Phone,
                CreatedAt = u.CreatedAt

            }).ToList();

        }

        public List<UserDTO> FindByUsername(string username)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(username))
                    return new List<UserDTO>();

                string keywordSlug = Helper.ConvertToSlug(username);

                var result = _context.Users
                    .AsEnumerable()  
                    .Where(u => !string.IsNullOrWhiteSpace(u.Username) &&
                                Helper.ConvertToSlug(u.Username).Contains(keywordSlug))
                    .Select(u => new UserDTO
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        Phone = u.Phone,
                        CreatedAt = u.CreatedAt
                    })
                    .ToList();

                return result;
            }

            catch (Exception ex)
            {

                Console.WriteLine("Error in FindByUsername: " + ex.Message);

                // 2️⃣ Trả về danh sách rỗng để không crash chương trình
                return new List<UserDTO>();

            }
        }
        public async Task SendOtpAsync(string email, string otp)
        {
            var message = new MailMessage();
            message.To.Add(email);
            message.Subject = "Mã OTP đặt lại mật khẩu";
            message.Body = $@"
                Xin chào,

                Mã OTP của bạn là: {otp}
                Mã có hiệu lực trong 5 phút.

                Nếu không phải bạn yêu cầu, hãy bỏ qua email này.
                ";

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("your@gmail.com", "app-password"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }

}

