using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Data;
using UserManagement.API.DTOs.UserDTOs;
using UserManagement.API.Helpers;
using UserManagement.API.Models;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // MARK: GET ALL USERS - PAGINATED / SEARCHABLE / SORTABLE
        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryDto filter)
        {
            var query = _context.Users
                .Include(x => x.Role)
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.Username.Contains(search) ||
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    x.Role.RoleName.Contains(search));
            }

            var sortBy = filter.SortBy.Trim().ToLower();
            var sortOrder = filter.SortOrder.Trim().ToLower();

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

            var result = await PaginationHelper.ToPagedResultAsync(
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

            return Ok(result);
        }

        // MARK: LIST ALL USERS - NO PAGINATION
        [HttpGet("list-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListAll()
        {
            var users = await _context.Users
                .Include(x => x.Role)
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
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

            return Ok(users);
        }

        // MARK: GET USER BY ID
        [HttpGet("get/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
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
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        // MARK: CREATE USER
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(UserCreateDto request)
        {
            var usernameExists = await _context.Users
                .AnyAsync(x => x.Username == request.Username && !x.IsDeleted);

            if (usernameExists)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var userRoleExists = await _context.Roles.AnyAsync(x => x.Id == 2);

            if (!userRoleExists)
            {
                return BadRequest(new { message = "Default User role not found." });
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully.",
                id = user.Id,
                username = user.Username,
                firstName = user.FirstName,
                lastName = user.LastName,
                status = user.Status,
                isDeleted = user.IsDeleted
            });
        }

        // MARK: UPDATE USER BY ADMIN
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var usernameExists = await _context.Users
                    .AnyAsync(x =>
                        x.Username == request.Username &&
                        x.Id != id &&
                        !x.IsDeleted);

                if (usernameExists)
                {
                    return BadRequest(new { message = "Username already exists." });
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
                var roleExists = await _context.Roles
                    .AnyAsync(x => x.Id == request.RoleId.Value);

                if (!roleExists)
                {
                    return BadRequest(new { message = "Role not found." });
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

                // Optional: force re-login after password update
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                user.LastActivityAt = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully.",
                data = new
                {
                    id = user.Id,
                    username = user.Username,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    roleId = user.RoleId,
                    status = user.Status,
                    isDeleted = user.IsDeleted
                }
            });
        }

        // MARK: UPDATE USER BY LOGGED IN USER
        [HttpPut("profile/update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                data = new
                {
                    id = user.Id,
                    username = user.Username,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    roleId = user.RoleId,
                    status = user.Status,
                    isDeleted = user.IsDeleted
                }
            });
        }

        // MARK: BULK SOFT DELETE USERS
        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete(UserBulkDeleteDto request)
        {
            if (request.Ids == null || !request.Ids.Any())
            {
                return BadRequest(new { message = "No user IDs provided." });
            }

            var users = await _context.Users
                .Where(x => request.Ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound(new { message = "No users found to delete." });
            }

            foreach (var user in users)
            {
                user.IsDeleted = true;
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Users deleted successfully.",
                deletedCount = users.Count
            });
        }
    }
}