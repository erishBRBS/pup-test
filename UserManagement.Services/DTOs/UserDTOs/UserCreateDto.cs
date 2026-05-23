using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.DTOs.UserDTOs
{
    // DTO for creating a new user
    public class UserCreateDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }
}

