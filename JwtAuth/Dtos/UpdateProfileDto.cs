namespace JwtAuth.Dtos
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}
