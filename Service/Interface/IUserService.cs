using Models.DTO;
using Models.Models;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IUserService
    {
        User GetByUsername(string username);
        bool Exists(string username, string email );
        void create(User user);

        Task<ResponseDTO<PagedResult<UserDTO>>> GetUsersAsync(UserQueryParameters parameters);
        Task<ResponseDTO<UserDTO>> CreateUserAsync(AdminCreateUserDTO dto);
        Task<ResponseDTO<UserDTO>> UpdateUserAsync(UpdateUserDTO dto);
        Task<ResponseDTO<bool>> UpdateUserStatusAsync(UserStatusUpdateDTO dto);

        Task SendOtpAsync(string email, string otp);

    }
}
