using UserManagement.API.Models;

namespace UserManagement.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetActiveUsersQuery();

        Task<User?> GetActiveByIdAsync(int id);

        Task<User?> GetByIdAsync(int id);

        Task<bool> UsernameExistsAsync(string username, int? excludedUserId = null);

        Task<bool> RoleExistsAsync(int roleId);

        Task<List<User>> GetActiveByIdsAsync(List<int> ids);

        Task AddAsync(User user);

        Task SaveChangesAsync();

        Task<User?> GetByUsernameWithRoleAsync(string username);

        Task<User?> GetByRefreshTokenWithRoleAsync(string refreshToken);
    }
}