using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.Services.DTOs.RoleDTOs;
using UserManagement.DAL.Models;
using UserManagement.DAL.Repositories.Interfaces;
using UserManagement.Services.Services.Interfaces;

namespace UserManagement.Services.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<RoleResponseDto>> ListAllAsync()
        {
            var roles = await _roleRepository.ListAllAsync();

            return roles
                .Select(ToResponse)
                .ToList();
        }

        public async Task<ServiceResultDto<RoleResponseDto>> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
            {
                return ServiceResultDto<RoleResponseDto>.NotFound("Role not found.");
            }

            return ServiceResultDto<RoleResponseDto>.Ok(
                "Role retrieved successfully.",
                ToResponse(role));
        }

        private static RoleResponseDto ToResponse(Role role)
        {
            return new RoleResponseDto
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
        }
    }
}

