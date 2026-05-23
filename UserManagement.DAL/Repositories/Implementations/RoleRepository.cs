using Microsoft.EntityFrameworkCore;
using UserManagement.DAL.Data;
using UserManagement.DAL.Models;
using UserManagement.DAL.Repositories.Interfaces;

namespace UserManagement.DAL.Repositories.Implementations
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

