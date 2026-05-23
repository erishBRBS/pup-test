using UserManagement.API.Models;

namespace UserManagement.API.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> ListAllAsync();

        Task<Role?> GetByIdAsync(int id);
    }
}