using Microsoft.EntityFrameworkCore;
using Models.Models;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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



    }

}

