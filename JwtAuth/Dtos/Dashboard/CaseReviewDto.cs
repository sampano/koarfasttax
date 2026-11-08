namespace JwtAuth.Dtos.Dashboard
{
    public class CaseReviewDto
    {
        public int TaxCaseId { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public int TaxYear { get; set; }

        public int UploadedDocumentsCount { get; set; }
    }
}