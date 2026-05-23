using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.DTOs.AuthDTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

