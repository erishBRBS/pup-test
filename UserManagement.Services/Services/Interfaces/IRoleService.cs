using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.Services.DTOs.RoleDTOs;

namespace UserManagement.Services.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> ListAllAsync();

        Task<ServiceResultDto<RoleResponseDto>> GetByIdAsync(int id);
    }
}

