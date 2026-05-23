namespace UserManagement.API.DTOs.UserDTOs
{
    public class UserQueryDto
    {
        public string? Search { get; set; }

        public string SortBy { get; set; } = "createdAt";

        public string SortOrder { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}