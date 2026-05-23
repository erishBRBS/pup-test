namespace UserManagement.Services.DTOs.AuthDTOs
{
    public class AuthResponseDto
    {
        public AuthUserDto User { get; set; } = new();

        public AuthTokenDto Token { get; set; } = new();
    }

    public class AuthRoleDto
    {
        public int Id { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }


    public class AuthUserDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public AuthRoleDto Role { get; set; } = new();

        public bool Status { get; set; }
    }

    public class AuthTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}

