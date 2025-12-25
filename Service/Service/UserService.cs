using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.Models;
using Service.Interface;
using Service.Utility;
using Helper.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
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
            var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Users");
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
        private static readonly Expression<Func<User, UserDTO>> UserProjection = user => new UserDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            RoleId = user.RoleId,
            RoleName = user.Role != null ? user.Role.RoleName : null,
            IsActive = user.Active ?? false,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        private Task<UserDTO?> GetUserDtoAsync(int userId)
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(UserProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<ResponseDTO<PagedResult<UserDTO>>> GetUsersAsync(UserQueryParameters parameters)
        {
            var safeParams = parameters ?? new UserQueryParameters();

            try
            {
                var query = _context.Users.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(safeParams.SearchTerm))
                {
                    var keyword = $"%{safeParams.SearchTerm.Trim()}%";
                    query = query.Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && EF.Functions.Like(u.Username, keyword)) ||
                        (!string.IsNullOrEmpty(u.Email) && EF.Functions.Like(u.Email, keyword)) ||
                        (!string.IsNullOrEmpty(u.Phone) && EF.Functions.Like(u.Phone, keyword)));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(u => u.CreatedAt ?? DateTime.MinValue)
                    .Skip(safeParams.Skip)
                    .Take(safeParams.PageSize)
                    .Select(UserProjection)
                    .ToListAsync();

                var payload = new PagedResult<UserDTO>
                {
                    Items = items,
                    PageNumber = safeParams.PageNumber,
                    PageSize = safeParams.PageSize,
                    TotalCount = totalCount
                };

                return new ResponseDTO<PagedResult<UserDTO>>(true, "Lấy danh sách người dùng thành công", payload, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetUsersAsync] {ex}");
                return new ResponseDTO<PagedResult<UserDTO>>(false, "Không thể lấy danh sách người dùng", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<UserDTO>> CreateUserAsync(RegisterDTO dto)
        {
            if (dto == null)
            {
                return new ResponseDTO<UserDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData);
            }

            try
            {
                var normalizedUsername = dto.Username.Trim();
                var normalizedEmail = dto.Email.Trim();
                var normalizedPhone = dto.Phone.Trim();

                var isDuplicated = await _context.Users.AnyAsync(u =>
                    u.Username == normalizedUsername ||
                    u.Email == normalizedEmail ||
                    u.Phone == normalizedPhone);

                if (isDuplicated)
                {
                    return new ResponseDTO<UserDTO>(false, "Username, Email hoặc Phone đã tồn tại", null, ResponseCode.UserExists);
                }

                var defaultRole = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RoleName == "User");

                if (defaultRole == null)
                {
                    return new ResponseDTO<UserDTO>(false, "Không tìm thấy role mặc định", null, ResponseCode.InvalidData);
                }

                var now = DateTime.UtcNow;

                var user = new User
                {
                    Username = normalizedUsername,
                    Email = normalizedEmail,
                    Phone = normalizedPhone,
                    Password = PasswordHelper.HashPassword(dto.Password),
                    RoleId = defaultRole.RoleId,
                    Active = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var createdUser = await GetUserDtoAsync(user.UserId);

                return new ResponseDTO<UserDTO>(true, "Tạo người dùng thành công", createdUser, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateUserAsync] {ex}");
                return new ResponseDTO<UserDTO>(false, "Không thể tạo người dùng", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<UserDTO>> UpdateUserAsync(UpdateUserDTO dto)
        {
            if (dto == null)
            {
                return new ResponseDTO<UserDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData);
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (user == null)
                {
                    return new ResponseDTO<UserDTO>(false, "Không tìm thấy người dùng", null, ResponseCode.InvalidData);
                }

                if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var normalizedUsername = dto.Username.Trim();
                    if (!normalizedUsername.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        var usernameExists = await _context.Users.AnyAsync(u =>
                            u.Username == normalizedUsername && u.UserId != dto.UserId);

                        if (usernameExists)
                        {
                            return new ResponseDTO<UserDTO>(false, "Username đã tồn tại", null, ResponseCode.UserExists);
                        }

                        user.Username = normalizedUsername;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var normalizedEmail = dto.Email.Trim();
                    if (!normalizedEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var emailExists = await _context.Users.AnyAsync(u =>
                            u.Email == normalizedEmail && u.UserId != dto.UserId);

                        if (emailExists)
                        {
                            return new ResponseDTO<UserDTO>(false, "Email đã tồn tại", null, ResponseCode.UserExists);
                        }

                        user.Email = normalizedEmail;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    var normalizedPhone = dto.Phone.Trim();
                    var currentPhone = user.Phone ?? string.Empty;

                    if (!normalizedPhone.Equals(currentPhone, StringComparison.Ordinal))
                    {
                        var phoneExists = await _context.Users.AnyAsync(u =>
                            u.Phone == normalizedPhone && u.UserId != dto.UserId);

                        if (phoneExists)
                        {
                            return new ResponseDTO<UserDTO>(false, "Số điện thoại đã tồn tại", null, ResponseCode.UserExists);
                        }

                        user.Phone = normalizedPhone;
                    }
                }

                if (dto.RoleId.HasValue && dto.RoleId.Value != user.RoleId)
                {
                    var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == dto.RoleId.Value);
                    if (!roleExists)
                    {
                        return new ResponseDTO<UserDTO>(false, "Role không tồn tại", null, ResponseCode.InvalidData);
                    }

                    user.RoleId = dto.RoleId.Value;
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedUser = await GetUserDtoAsync(user.UserId);

                return new ResponseDTO<UserDTO>(true, "Cập nhật người dùng thành công", updatedUser, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUserAsync] {ex}");
                return new ResponseDTO<UserDTO>(false, "Không thể cập nhật người dùng", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<bool>> UpdateUserStatusAsync(UserStatusUpdateDTO dto)
        {
            if (dto == null)
            {
                return new ResponseDTO<bool>(false, "Dữ liệu không hợp lệ", false, ResponseCode.InvalidData);
            }

            if (!dto.IsActive.HasValue)
            {
                return new ResponseDTO<bool>(false, "Trạng thái không hợp lệ", false, ResponseCode.InvalidData);
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (user == null)
                {
                    return new ResponseDTO<bool>(false, "Không tìm thấy người dùng", false, ResponseCode.InvalidData);
                }

                user.Active = dto.IsActive.Value;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var message = dto.IsActive.Value ? "Đã bỏ chặn người dùng" : "Đã chặn người dùng";
                return new ResponseDTO<bool>(true, message, dto.IsActive.Value, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUserStatusAsync] {ex}");
                return new ResponseDTO<bool>(false, "Không thể cập nhật trạng thái người dùng", false, ResponseCode.ServerError);
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

