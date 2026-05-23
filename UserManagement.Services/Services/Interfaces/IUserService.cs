using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.Services.DTOs.UserDTOs;

namespace UserManagement.Services.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResultDto<UserResponseDto>> GetAllAsync(UserQueryDto filter);

        Task<List<UserResponseDto>> ListAllAsync();

        Task<ServiceResultDto<UserResponseDto>> GetByIdAsync(int id);

        Task<ServiceResultDto<UserMutationResponseDto>> CreateAsync(UserCreateDto request);

        Task<ServiceResultDto<UserMutationResponseDto>> UpdateAsync(int id, UserUpdateDto request);

        Task<ServiceResultDto<UserMutationResponseDto>> UpdateProfileAsync(int userId, UserProfileUpdateDto request);

        Task<ServiceResultDto<UserBulkDeleteResponseDto>> BulkDeleteAsync(UserBulkDeleteDto request);
    }
}

