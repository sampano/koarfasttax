namespace JwtAuth.Dtos
{
    public class ProfileDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? ProfileImageUrl { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
