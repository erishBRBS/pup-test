namespace UserManagement.API.DTOs.UserDTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public UserRoleResponseDto Role { get; set; } = new();

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class UserRoleResponseDto
    {
        public int Id { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}