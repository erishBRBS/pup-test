using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs.AuthDTOs
{
    public class AuthRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}