using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagement.API.DTOs.AuthDTOs;
using UserManagement.API.DTOs.CommonDTOs;
using UserManagement.API.Services.Interfaces;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // MARK: LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return ToActionResult(result);
        }

        // MARK: REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshAsync(request);
            return ToActionResult(result);
        }

        // MARK: CHANGE PASSWORD
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var userId = int.Parse(userIdClaim);

            var result = await _authService.ChangePasswordAsync(userId, request);
            return ToActionResult(result);
        }

        // MARK: LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.LogoutAsync(request);
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