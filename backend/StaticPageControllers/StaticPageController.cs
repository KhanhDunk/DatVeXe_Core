using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Service.Interface;

namespace backend.StaticPageControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticPageController : ControllerBase
    {
        private readonly IStaticPageService _staticPageService;

        public StaticPageController(IStaticPageService staticPageService)
        {
            _staticPageService = staticPageService;
        }

        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<StaticPageDTO>>> GetBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new ResponseDTO<StaticPageDTO>(false, "Slug không hợp lệ", null, ResponseCode.InvalidData));
                }

                var result = await _staticPageService.GetBySlugAsync(slug);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.GetBySlug] {ex}");
                var error = new ResponseDTO<StaticPageDTO>(false, "Không thể xử lý yêu cầu", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<List<StaticPageDTO>>>> GetPublished()
        {
            try
            {
                var result = await _staticPageService.GetPublishedAsync();
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.GetPublished] {ex}");
                var error = new ResponseDTO<List<StaticPageDTO>>(false, "Không thể xử lý yêu cầu", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<List<StaticPageDTO>>>> GetAll()
        {
            try
            {
                var result = await _staticPageService.GetAllAsync();
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.GetAll] {ex}");
                var error = new ResponseDTO<List<StaticPageDTO>>(false, "Không thể xử lý yêu cầu", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ResponseDTO<StaticPageDTO>>> Create([FromBody] StaticPageCreateDTO request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO<StaticPageDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData));
                }

                var userId = GetUserIdFromClaims();
                var result = await _staticPageService.CreateAsync(request, userId);
                return BuildResponse(result, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.Create] {ex}");
                var error = new ResponseDTO<StaticPageDTO>(false, "Không thể xử lý yêu cầu", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ResponseDTO<StaticPageDTO>>> Update(int id, [FromBody] StaticPageUpdateDTO request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO<StaticPageDTO>(false, "Dữ liệu không hợp lệ", null, ResponseCode.InvalidData));
                }

                request.PageId = id;
                var isSuperAdmin = User.IsInRole("SuperAdmin");

                if (!isSuperAdmin && (request.Title != null || request.Slug != null))
                {
                    var forbidden = new ResponseDTO<StaticPageDTO>(false, "Bạn không có quyền thay đổi cấu trúc đường dẫn của trang", null, ResponseCode.Forbidden);
                    return StatusCode(StatusCodes.Status403Forbidden, forbidden);
                }

                var userId = GetUserIdFromClaims();
                var result = await _staticPageService.UpdateAsync(id, request, isSuperAdmin, userId);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.Update] {ex}");
                var error = new ResponseDTO<StaticPageDTO>(false, "Không thể xử lý yêu cầu", null, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var result = await _staticPageService.DeleteAsync(id);
                return BuildResponse(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageController.Delete] {ex}");
                var error = new ResponseDTO<bool>(false, "Không thể xử lý yêu cầu", false, ResponseCode.ServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        private int? GetUserIdFromClaims()
        {
            var userIdValue = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }

        private ActionResult<ResponseDTO<T>> BuildResponse<T>(ResponseDTO<T> response, bool created = false)
        {
            return response.Code switch
            {
                ResponseCode.Success when created => StatusCode(StatusCodes.Status201Created, response),
                ResponseCode.Success => Ok(response),
                ResponseCode.InvalidData => BadRequest(response),
                ResponseCode.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                ResponseCode.NotFound => NotFound(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
