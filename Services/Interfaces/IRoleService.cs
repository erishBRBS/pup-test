using UserManagement.API.DTOs.CommonDTOs;
using UserManagement.API.DTOs.RoleDTOs;

namespace UserManagement.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> ListAllAsync();

        Task<ServiceResultDto<RoleResponseDto>> GetByIdAsync(int id);
    }
}