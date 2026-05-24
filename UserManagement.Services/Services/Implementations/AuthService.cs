using Microsoft.Extensions.Configuration;
using UserManagement.Services.DTOs.AuthDTOs;
using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.DAL.Models;
using UserManagement.DAL.Repositories.Interfaces;
using UserManagement.Services.Services.Auth;
using UserManagement.Services.Services.Interfaces;

namespace UserManagement.Services.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<ServiceResultDto<AuthResponseDto>> LoginAsync(AuthRequestDto request)
        {
            var user = await _userRepository.GetByUsernameWithRoleAsync(request.Username);

            if (user == null)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Invalid username or password.");
            }

            if (user.IsDeleted)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Account does not exist.");
            }

            if (!user.Status)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Account is inactive.");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordValid)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Invalid username or password.");
            }

            var accessTokenExpiresAt = _jwtService.GetAccessTokenExpiration();
            var accessToken = _jwtService.GenerateAccessToken(user, accessTokenExpiresAt);

            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
            user.LastActivityAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<AuthResponseDto>.Ok(
                "Login successful.",
                ToAuthResponse(user, accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt));
        }

        public async Task<ServiceResultDto<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetByRefreshTokenWithRoleAsync(request.RefreshToken);

            if (user == null)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Invalid refresh token.");
            }

            if (user.IsDeleted)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Account does not exist.");
            }

            if (!user.Status)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Account is inactive.");
            }

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Refresh token expired.");
            }

            if (!user.LastActivityAt.HasValue)
            {
                return ServiceResultDto<AuthResponseDto>.Unauthorized("Session expired. Please login again.");
            }

            var idleTimeoutMinutes = _configuration.GetValue<int>("Jwt:IdleTimeoutMinutes", 15);

            if ((DateTime.UtcNow - user.LastActivityAt.Value).TotalMinutes > idleTimeoutMinutes)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;

                await _userRepository.SaveChangesAsync();

                return ServiceResultDto<AuthResponseDto>.Unauthorized("Session expired due to inactivity.");
            }

            var accessTokenExpiresAt = _jwtService.GetAccessTokenExpiration();
            var accessToken = _jwtService.GenerateAccessToken(user, accessTokenExpiresAt);

            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = newRefreshTokenExpiresAt;
            user.LastActivityAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<AuthResponseDto>.Ok(
                "Token refreshed successfully.",
                ToAuthResponse(user, accessToken, accessTokenExpiresAt, newRefreshToken, newRefreshTokenExpiresAt));
        }

        public async Task<ServiceResultDto<object>> ChangePasswordAsync(
            int userId,
            ChangePasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return ServiceResultDto<object>.BadRequest("New password and confirm password do not match.");
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.IsDeleted)
            {
                return ServiceResultDto<object>.NotFound("User not found.");
            }

            if (!user.Status)
            {
                return ServiceResultDto<object>.Unauthorized("Account is inactive.");
            }

            var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);

            if (!isOldPasswordValid)
            {
                return ServiceResultDto<object>.BadRequest("Old password is incorrect.");
            }

            var isSamePassword = BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password);

            if (isSamePassword)
            {
                return ServiceResultDto<object>.BadRequest("New password must be different from old password.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.LastActivityAt = null;

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<object>.Ok("Password changed successfully. Please login again.");
        }

        public async Task<ServiceResultDto<object>> LogoutAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetByRefreshTokenWithRoleAsync(request.RefreshToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;

                await _userRepository.SaveChangesAsync();
            }

            return ServiceResultDto<object>.Ok("Logged out successfully.");
        }

        private static AuthResponseDto ToAuthResponse(
            User user,
            string accessToken,
            DateTime accessTokenExpiresAt,
            string refreshToken,
            DateTime refreshTokenExpiresAt)
        {
            return new AuthResponseDto
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
                    // RefreshToken = refreshToken,
                    // RefreshTokenExpiresAt = refreshTokenExpiresAt
                }
            };
        }
    }
}


