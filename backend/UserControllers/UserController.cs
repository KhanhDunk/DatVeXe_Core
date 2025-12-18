using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Service.Interface;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {

        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService; 
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getUsers")]
        public ActionResult<ResponseDTO<List<UserDTO>>> GetUsers()
        {
            var users = _userService.GetAll();

            return Ok(new ResponseDTO<List<UserDTO>>(
                success : true,
                message :  "Lấy danh sách user thành công",
                data: users
            ));
        }

        /// <summary>
        /// Tìm kiếm user theo username
        /// </summary>
        //[Authorize(Roles = "Admin,User")] // Admin hoặc User đều được
        [HttpGet("search")]
        public ActionResult<ResponseDTO<List<UserDTO>>> SearchUser([FromQuery] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(new ResponseDTO<List<UserDTO>>(
                        success: false,
                        message: "Username không được để trống",
                        data: null
                    ));
                }

                // Gọi service tìm kiếm
                var users = _userService.FindByUsername(username);

                if (users == null || users.Count == 0)
                {
                    return NotFound(new ResponseDTO<List<UserDTO>>(
                        success: false,
                        message: "Không tìm thấy user nào",
                        data: null
                    ));
                }

                return Ok(new ResponseDTO<List<UserDTO>>(
                    success: true,
                    message: "Tìm kiếm user thành công",
                    data: users
                ));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, new ResponseDTO<List<UserDTO>>(
                    success: false,
                    message: "Lỗi server: " + ex.Message,
                    data: null
                ));
            }
        }
    }


}

