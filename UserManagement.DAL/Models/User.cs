using System.ComponentModel.DataAnnotations;

namespace UserManagement.DAL.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiresAt { get; set; }

        public bool Status { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime? LastActivityAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

