using UserManagement.Services.DTOs.AuthDTOs;
using UserManagement.Services.DTOs.CommonDTOs;

namespace UserManagement.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResultDto<AuthResponseDto>> LoginAsync(AuthRequestDto request);

        Task<ServiceResultDto<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request);

        Task<ServiceResultDto<object>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        Task<ServiceResultDto<object>> LogoutAsync(RefreshTokenRequestDto request);
    }
}

