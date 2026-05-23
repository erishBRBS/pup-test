using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UserManagement.API.Data;
using UserManagement.API.DTOs.AuthDTOs;
using UserManagement.API.Services.Auth;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        // MARK: LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthRequestDto request)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (user.IsDeleted)
            {
                return Unauthorized(new { message = "Account does not exist." });
            }

            if (!user.Status)
            {
                return Unauthorized(new { message = "Account is inactive." });
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var accessTokenExpiresAt = _jwtService.GetAccessTokenExpiration();
            var accessToken = _jwtService.GenerateAccessToken(user, accessTokenExpiresAt);

            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
            user.LastActivityAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    Role = new AuthRoleDto
                    {
                        Id = user.Role.Id,
                        RoleName = user.Role.RoleName
                    }
                },

                Token = new AuthTokenDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = refreshTokenExpiresAt
                }
            });
        }

        // MARK: REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            if (user.IsDeleted)
            {
                return Unauthorized(new { message = "Account does not exist." });
            }

            if (!user.Status)
            {
                return Unauthorized(new { message = "Account is inactive." });
            }

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh token expired." });
            }

            if (!user.LastActivityAt.HasValue)
            {
                return Unauthorized(new { message = "Session expired. Please login again." });
            }

            var idleTimeoutMinutes = _configuration.GetValue<int>("Jwt:IdleTimeoutMinutes", 15);

            if (user.LastActivityAt.HasValue &&
                (DateTime.UtcNow - user.LastActivityAt.Value).TotalMinutes > idleTimeoutMinutes)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;

                await _context.SaveChangesAsync();

                return Unauthorized(new { message = "Session expired due to inactivity." });
            }

            var accessTokenExpiresAt = _jwtService.GetAccessTokenExpiration();
            var accessToken = _jwtService.GenerateAccessToken(user, accessTokenExpiresAt);

            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = newRefreshTokenExpiresAt;
            user.LastActivityAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    Role = new AuthRoleDto
                    {
                        Id = user.Role.Id,
                        RoleName = user.Role.RoleName
                    }
                },

                Token = new AuthTokenDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiresAt = newRefreshTokenExpiresAt
                }
            });
        }

        // MARK: CHANGE PASSWORD
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "New password and confirm password do not match." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var userId = int.Parse(userIdClaim);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!user.Status)
            {
                return Unauthorized(new { message = "Account is inactive." });
            }

            var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);

            if (!isOldPasswordValid)
            {
                return BadRequest(new { message = "Old password is incorrect." });
            }

            var isSamePassword = BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password);

            if (isSamePassword)
            {
                return BadRequest(new { message = "New password must be different from old password." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.LastActivityAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully. Please login again." });
        }

        // MARK: LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully." });
        }
    }
}