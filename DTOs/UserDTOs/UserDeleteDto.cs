using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs.UserDTOs
{
    public class UserBulkDeleteDto
    {
        [Required]
        public List<int> Ids { get; set; } = new();
    }
}