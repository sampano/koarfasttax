namespace JwtAuth.Dtos.TaxCases
{
    public class TaxCaseActivityDto
    {
        public int Id { get; set; }

        public int TaxCaseId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string UserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}