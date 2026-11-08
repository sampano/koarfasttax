using JwtAuth.Entities;

namespace JwtAuth.Dtos.DocumentRequests
{
    public class DocumentRequestDto
    {
        public int Id { get; set; }

        public int TaxCaseId { get; set; }

        public int DocumentTypeId { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string DocumentCategory { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DocumentRequestStatus Status { get; set; }
           = DocumentRequestStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
