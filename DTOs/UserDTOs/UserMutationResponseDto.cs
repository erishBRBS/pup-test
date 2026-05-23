namespace UserManagement.API.DTOs.UserDTOs
{
    public class UserMutationResponseDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public bool Status { get; set; }

        public bool IsDeleted { get; set; }
    }
}