using UserManagement.DAL.Models;

namespace UserManagement.DAL.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> ListAllAsync();

        Task<Role?> GetByIdAsync(int id);
    }
}

