namespace JwtAuth.Entities
{
    public class TaxCaseActivity
    {
        public int Id { get; set; }

        public int TaxCaseId { get; set; }

        public TaxCase TaxCase { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}