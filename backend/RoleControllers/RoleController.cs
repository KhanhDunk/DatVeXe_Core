using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Service.Interface;

namespace backend.RoleControllers
{
    [Authorize(Roles = "SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<List<RoleDTO>>>> GetAll()
        {
            try
            {
                var result = await _roleService.GetAllAsync();
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleController.GetAll] {ex}");
                var error = new ResponseDTO<List<RoleDTO>>(false, "Kh\u00F4ng th\u1EC3 x\u1EED l\u00FD y\u00EAu c\u1EA7u", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResponseDTO<RoleDTO>>> GetById(int id)
        {
            try
            {
                var result = await _roleService.GetByIdAsync(id);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleController.GetById] {ex}");
                var error = new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 x\u1EED l\u00FD y\u00EAu c\u1EA7u", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<RoleDTO>>> Create([FromBody] RoleCreateDTO request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO<RoleDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData));
                }

                var result = await _roleService.CreateAsync(request);
                return BuildResponse(result, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleController.Create] {ex}");
                var error = new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 x\u1EED l\u00FD y\u00EAu c\u1EA7u", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseDTO<RoleDTO>>> Update(int id, [FromBody] RoleUpdateDTO request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO<RoleDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData));
                }

                request.RoleId = id;
                var result = await _roleService.UpdateAsync(id, request);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleController.Update] {ex}");
                var error = new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 x\u1EED l\u00FD y\u00EAu c\u1EA7u", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleController.Delete] {ex}");
                var error = new ResponseDTO<bool>(false, "Kh\u00F4ng th\u1EC3 x\u1EED l\u00FD y\u00EAu c\u1EA7u", false, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        private ActionResult<ResponseDTO<T>> BuildResponse<T>(ResponseDTO<T> response, bool created = false)
        {
            return response.Code switch
            {
                ResponseCode.Success when created => StatusCode(StatusCodes.Status201Created, response),
                ResponseCode.Success => Ok(response),
                ResponseCode.InvalidData => BadRequest(response),
                ResponseCode.NotFound => NotFound(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
