using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs.CommonDTOs;
using UserManagement.API.Services.Interfaces;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: /api/roles/list-all
        [HttpGet]
        [HttpGet("list-all")]
        public async Task<IActionResult> ListAll()
        {
            var result = await _roleService.ListAllAsync();
            return Ok(result);
        }

        // GET: /api/roles/get/1
        // [HttpGet("get/{id:int}")]
        // public async Task<IActionResult> GetById(int id)
        // {
        //     var result = await _roleService.GetByIdAsync(id);
        //     return ToActionResult(result);
        // }

        private IActionResult ToActionResult<T>(ServiceResultDto<T> result)
        {
            var response = new
            {
                message = result.Message,
                data = result.Data
            };

            return result.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(new { message = result.Message }),
                401 => Unauthorized(new { message = result.Message }),
                404 => NotFound(new { message = result.Message }),
                _ => StatusCode(result.StatusCode, response)
            };
        }
    }
}