using Microsoft.EntityFrameworkCore;
using UserManagement.API.Data;
using UserManagement.API.Models;
using UserManagement.API.Repositories.Interfaces;

namespace UserManagement.API.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> ListAllAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}