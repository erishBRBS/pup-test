using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.DTOs.UserDTOs
{
    public class UserUpdateDto
    {
        public string? Username { get; set; } = string.Empty;

        public string? Password { get; set; }

        public string? FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; } = string.Empty;

        public bool? Status { get; set; }

        public int? RoleId { get; set; }
    }
}

