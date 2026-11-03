namespace JwtAuth.Dtos
{
    public class GoHighLevelContactDto
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public bool IsImported { get; set; }
    }
}