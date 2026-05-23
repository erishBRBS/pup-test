using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs.AuthDTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}