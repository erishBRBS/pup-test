using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.Services.DTOs.UserDTOs;
using UserManagement.Services.Services.Interfaces;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // MARK: GET ALL USERS - PAGINATED / SEARCHABLE / SORTABLE
        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryDto filter)
        {
            var result = await _userService.GetAllAsync(filter);
            return Ok(result);
        }

        // MARK: LIST ALL USERS - NO PAGINATION
        [HttpGet("list-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListAll()
        {
            var result = await _userService.ListAllAsync();
            return Ok(result);
        }

        // MARK: GET USER BY ID
        [HttpGet("get/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            return ToActionResult(result);
        }

        // MARK: CREATE USER
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto request)
        {
            var result = await _userService.CreateAsync(request);
            return ToActionResult(result);
        }

        // MARK: UPDATE USER BY ADMIN
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto request)
        {
            var result = await _userService.UpdateAsync(id, request);
            return ToActionResult(result);
        }

        // MARK: UPDATE USER BY LOGGED IN USER
        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var userId = int.Parse(userIdClaim);

            var result = await _userService.UpdateProfileAsync(userId, request);
            return ToActionResult(result);
        }

        // MARK: BULK SOFT DELETE USERS
        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete([FromBody] UserBulkDeleteDto request)
        {
            var result = await _userService.BulkDeleteAsync(request);
            return ToActionResult(result);
        }

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

