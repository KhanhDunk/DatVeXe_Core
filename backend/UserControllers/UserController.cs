using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Service.Interface;
using Helper.Enums;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<PagedResult<UserDTO>>>> GetUsers([FromQuery] UserQueryParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<PagedResult<UserDTO>>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData));
            }

            var result = await _userService.GetUsersAsync(parameters ?? new UserQueryParameters());

            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> CreateUser([FromBody] RegisterDTO request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<UserDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData));
            }

            var result = await _userService.CreateUserAsync(request);

            if (!result.Success)
            {
                return result.Code switch
                {
                    ResponseCode.UserExists => Conflict(result),
                    ResponseCode.InvalidData => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result)
                };
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{userId:int}")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> UpdateUser(int userId, [FromBody] UpdateUserDTO request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<UserDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData));
            }

            request.UserId = userId;

            var result = await _userService.UpdateUserAsync(request);

            if (!result.Success)
            {
                return result.Code switch
                {
                    ResponseCode.UserExists => Conflict(result),
                    ResponseCode.InvalidData => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result)
                };
            }

            return Ok(result);
        }

        [HttpPatch("{userId:int}/status")]
        public async Task<ActionResult<ResponseDTO<bool>>> UpdateStatus(int userId, [FromBody] UserStatusUpdateDTO request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<bool>(false, "Dữ liệu không hợp lệ", false, ResponseCode.InvalidData));
            }

            request.UserId = userId;

            var result = await _userService.UpdateUserStatusAsync(request);

            if (!result.Success)
            {
                return result.Code switch
                {
                    ResponseCode.InvalidData => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result)
                };
            }

            return Ok(result);
        }
    }
}

