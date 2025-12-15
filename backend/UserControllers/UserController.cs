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




    }
}
