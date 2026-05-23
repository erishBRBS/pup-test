using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.DTOs.UserDTOs
{
    public class UserBulkDeleteDto
    {
        [Required]
        public List<int> Ids { get; set; } = new();
    }
}

