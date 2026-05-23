using Microsoft.EntityFrameworkCore;
using UserManagement.API.Data;
using UserManagement.API.Models;
using UserManagement.API.Repositories.Interfaces;

namespace UserManagement.API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetActiveUsersQuery()
        {
            return _context.Users
                .Include(x => x.Role)
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .AsQueryable();
        }

        public async Task<User?> GetActiveByIdAsync(int id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludedUserId = null)
        {
            var query = _context.Users
                .Where(x => x.Username == username && !x.IsDeleted);

            if (excludedUserId.HasValue)
            {
                query = query.Where(x => x.Id != excludedUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> RoleExistsAsync(int roleId)
        {
            return await _context.Roles
                .AnyAsync(x => x.Id == roleId);
        }

        public async Task<List<User>> GetActiveByIdsAsync(List<int> ids)
        {
            return await _context.Users
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}