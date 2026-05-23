using UserManagement.API.DTOs.AuthDTOs;
using UserManagement.API.DTOs.CommonDTOs;

namespace UserManagement.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResultDto<AuthResponseDto>> LoginAsync(AuthRequestDto request);

        Task<ServiceResultDto<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request);

        Task<ServiceResultDto<object>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        Task<ServiceResultDto<object>> LogoutAsync(RefreshTokenRequestDto request);
    }
}