using Microsoft.EntityFrameworkCore;
using UserManagement.Services.DTOs.CommonDTOs;
using UserManagement.Services.DTOs.UserDTOs;
using UserManagement.Services.Helpers;
using UserManagement.DAL.Models;
using UserManagement.DAL.Repositories.Interfaces;
using UserManagement.Services.Services.Interfaces;

namespace UserManagement.Services.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedResultDto<UserResponseDto>> GetAllAsync(UserQueryDto filter)
        {
            filter ??= new UserQueryDto();

            var query = _userRepository.GetActiveUsersQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.Username.Contains(search) ||
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    x.Role.RoleName.Contains(search));
            }

            var sortBy = (filter.SortBy ?? "createdAt").Trim().ToLower();
            var sortOrder = (filter.SortOrder ?? "desc").Trim().ToLower();

            query = (sortBy, sortOrder) switch
            {
                ("username", "asc") => query.OrderBy(x => x.Username),
                ("username", "desc") => query.OrderByDescending(x => x.Username),

                ("firstname", "asc") => query.OrderBy(x => x.FirstName),
                ("firstname", "desc") => query.OrderByDescending(x => x.FirstName),

                ("lastname", "asc") => query.OrderBy(x => x.LastName),
                ("lastname", "desc") => query.OrderByDescending(x => x.LastName),

                ("rolename", "asc") => query.OrderBy(x => x.Role.RoleName),
                ("rolename", "desc") => query.OrderByDescending(x => x.Role.RoleName),

                ("role", "asc") => query.OrderBy(x => x.Role.RoleName),
                ("role", "desc") => query.OrderByDescending(x => x.Role.RoleName),

                ("status", "asc") => query.OrderBy(x => x.Status),
                ("status", "desc") => query.OrderByDescending(x => x.Status),

                ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            return await PaginationHelper.ToPagedResultAsync(
                query,
                filter.PageNumber,
                filter.PageSize,
                x => new UserResponseDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    Role = new UserRoleResponseDto
                    {
                        Id = x.Role.Id,
                        RoleName = x.Role.RoleName
                    }
                });
        }

        public async Task<List<UserResponseDto>> ListAllAsync()
        {
            return await _userRepository.GetActiveUsersQuery()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new UserResponseDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    Role = new UserRoleResponseDto
                    {
                        Id = x.Role.Id,
                        RoleName = x.Role.RoleName
                    }
                })
                .ToListAsync();
        }

        public async Task<ServiceResultDto<UserResponseDto>> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetActiveByIdAsync(id);

            if (user == null)
            {
                return ServiceResultDto<UserResponseDto>.NotFound("User not found.");
            }

            var response = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                Role = new UserRoleResponseDto
                {
                    Id = user.Role.Id,
                    RoleName = user.Role.RoleName
                }
            };

            return ServiceResultDto<UserResponseDto>.Ok("User retrieved successfully.", response);
        }

        public async Task<ServiceResultDto<UserMutationResponseDto>> CreateAsync(UserCreateDto request)
        {
            if (request == null)
            {
                return ServiceResultDto<UserMutationResponseDto>.BadRequest("Request body is required.");
            }

            var usernameExists = await _userRepository.UsernameExistsAsync(request.Username);

            if (usernameExists)
            {
                return ServiceResultDto<UserMutationResponseDto>.BadRequest("Username already exists.");
            }

            var userRoleExists = await _userRepository.RoleExistsAsync(2);

            if (!userRoleExists)
            {
                return ServiceResultDto<UserMutationResponseDto>.BadRequest("Default User role not found.");
            }

            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleId = 2,
                Status = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<UserMutationResponseDto>.Ok(
                "User created successfully.",
                ToMutationResponse(user));
        }

        public async Task<ServiceResultDto<UserMutationResponseDto>> UpdateAsync(int id, UserUpdateDto request)
        {
            if (request == null)
            {
                return ServiceResultDto<UserMutationResponseDto>.BadRequest("Request body is required.");
            }

            var user = await _userRepository.GetActiveByIdAsync(id);

            if (user == null)
            {
                return ServiceResultDto<UserMutationResponseDto>.NotFound("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var usernameExists = await _userRepository.UsernameExistsAsync(request.Username, id);

                if (usernameExists)
                {
                    return ServiceResultDto<UserMutationResponseDto>.BadRequest("Username already exists.");
                }

                user.Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
            }

            if (request.RoleId.HasValue)
            {
                var roleExists = await _userRepository.RoleExistsAsync(request.RoleId.Value);

                if (!roleExists)
                {
                    return ServiceResultDto<UserMutationResponseDto>.BadRequest("Role not found.");
                }

                user.RoleId = request.RoleId.Value;
            }

            if (request.Status.HasValue)
            {
                user.Status = request.Status.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;
            }

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<UserMutationResponseDto>.Ok(
                "User updated successfully.",
                ToMutationResponse(user));
        }

        public async Task<ServiceResultDto<UserMutationResponseDto>> UpdateProfileAsync(
            int userId,
            UserProfileUpdateDto request)
        {
            if (request == null)
            {
                return ServiceResultDto<UserMutationResponseDto>.BadRequest("Request body is required.");
            }

            var user = await _userRepository.GetActiveByIdAsync(userId);

            if (user == null)
            {
                return ServiceResultDto<UserMutationResponseDto>.NotFound("User not found.");
            }

            if (!user.Status)
            {
                return ServiceResultDto<UserMutationResponseDto>.Unauthorized("Account is inactive.");
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
            }

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<UserMutationResponseDto>.Ok(
                "Profile updated successfully.",
                ToMutationResponse(user));
        }

        public async Task<ServiceResultDto<UserBulkDeleteResponseDto>> BulkDeleteAsync(UserBulkDeleteDto request)
        {
            if (request == null || request.Ids == null || !request.Ids.Any())
            {
                return ServiceResultDto<UserBulkDeleteResponseDto>.BadRequest("No user IDs provided.");
            }

            var users = await _userRepository.GetActiveByIdsAsync(request.Ids);

            if (!users.Any())
            {
                return ServiceResultDto<UserBulkDeleteResponseDto>.NotFound("No users found to delete.");
            }

            foreach (var user in users)
            {
                user.IsDeleted = true;
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;
            }

            await _userRepository.SaveChangesAsync();

            return ServiceResultDto<UserBulkDeleteResponseDto>.Ok(
                "Users deleted successfully.",
                new UserBulkDeleteResponseDto
                {
                    DeletedCount = users.Count
                });
        }

        private static UserMutationResponseDto ToMutationResponse(User user)
        {
            return new UserMutationResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                Status = user.Status,
                IsDeleted = user.IsDeleted
            };
        }
    }
}

