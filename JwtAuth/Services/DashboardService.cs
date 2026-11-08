using JwtAuth.Data;
using JwtAuth.Dtos.Dashboard;
using JwtAuth.Entities;
using JwtAuth.Enums;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
        {
            int totalClients = await _context.Clients.CountAsync(client =>
                    client.UserId == userId);

            int totalTaxCases = await _context.TaxCases.CountAsync(taxCase =>
                    taxCase.Client.UserId == userId);

            int waitingForDocumentsCount = await _context.TaxCases.CountAsync(taxCase =>
                    taxCase.Client.UserId == userId &&
                    taxCase.Status == TaxCaseStatus.WaitingForDocuments);

            int completedTaxCasesCount = await _context.TaxCases.CountAsync(taxCase =>
                    taxCase.Client.UserId == userId &&
                    taxCase.Status == TaxCaseStatus.Completed);

            int totalRequestedDocuments = await _context.DocumentRequests.CountAsync(request =>
                    request.TaxCase.Client.UserId == userId);

            int totalCompletedDocuments = await _context.DocumentRequests.CountAsync(request =>
                request.TaxCase.Client.UserId == userId &&
                request.Status == DocumentRequestStatus.Completed);

            int totalMissingDocuments = await _context.DocumentRequests.CountAsync(request =>
                request.TaxCase.Client.UserId == userId &&
                request.Status == DocumentRequestStatus.Pending);

            int totalUploadedDocuments = await _context.DocumentRequests.CountAsync(request =>
                request.TaxCase.Client.UserId == userId &&
                request.Status == DocumentRequestStatus.Uploaded);

            double overallProgress =
                totalRequestedDocuments == 0
                    ? 0
                    : Math.Round(
                        (double)totalCompletedDocuments /
                        totalRequestedDocuments * 100,
                        2);

            List<CaseAttentionDto> casesNeedingAttention =
                await _context.DocumentRequests
                    .Where(request =>
                        request.TaxCase.Client.UserId == userId &&
                        request.Status == DocumentRequestStatus.Pending)
                    .GroupBy(request => new
                    {
                        request.TaxCaseId,
                        request.TaxCase.ClientId,
                        request.TaxCase.Client.FirstName,
                        request.TaxCase.Client.LastName,
                        request.TaxCase.TaxYear
                    })
                    .Select(group => new CaseAttentionDto
                    {
                        TaxCaseId = group.Key.TaxCaseId,

                        ClientId = group.Key.ClientId,

                        ClientName =
                            group.Key.FirstName + " " + group.Key.LastName,

                        TaxYear = group.Key.TaxYear,

                        MissingDocumentsCount = group.Count()
                    })
                    .OrderByDescending(item =>
                        item.MissingDocumentsCount)
                    .Take(5)
                    .ToListAsync();

            List<CaseReviewDto> casesNeedingReview =
                await _context.DocumentRequests
                    .Where(request =>
                        request.TaxCase.Client.UserId == userId &&
                        request.Status == DocumentRequestStatus.Uploaded)
                    .GroupBy(request => new
                    {
                        request.TaxCaseId,
                        request.TaxCase.ClientId,
                        request.TaxCase.Client.FirstName,
                        request.TaxCase.Client.LastName,
                        request.TaxCase.TaxYear
                    })
                    .Select(group => new CaseReviewDto
                    {
                        TaxCaseId = group.Key.TaxCaseId,

                        ClientId = group.Key.ClientId,

                        ClientName =
                            group.Key.FirstName + " " + group.Key.LastName,

                        TaxYear = group.Key.TaxYear,

                        UploadedDocumentsCount = group.Count()
                    })
                    .OrderByDescending(item =>
                        item.UploadedDocumentsCount)
                    .Take(5)
                    .ToListAsync();

            List<RecentActivityDto> recentActivity = await _context.Documents
            .Where(document =>
                document.TaxCase.Client.UserId == userId)
            .OrderByDescending(document =>
                document.UploadedAt)
            .Take(5)
            .Select(document => new RecentActivityDto
            {
                DocumentId = document.Id,

                FileName = document.FileName,

                DocumentType = document.DocumentType != null ? document.DocumentType.Name : null,

                ClientName = document.TaxCase.Client.FirstName + " " + document.TaxCase.Client.LastName,

                TaxCaseId = document.TaxCaseId,

                TaxYear = document.TaxCase.TaxYear,

                UploadedAt = document.UploadedAt
            }).ToListAsync();


            List<TaxCaseStatusCountDto> taxCaseStatusBreakdown = await _context.TaxCases
            .Where(taxCase => taxCase.Client.UserId == userId)
            .GroupBy(taxCase => taxCase.Status)
            .Select(group => new TaxCaseStatusCountDto
            {
                Status = group.Key,
                Count = group.Count()
            })
            .OrderBy(status => status.Status)
            .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalClients = totalClients,
                TotalTaxCases = totalTaxCases,
                WaitingForDocumentsCount = waitingForDocumentsCount,
                CompletedTaxCasesCount = completedTaxCasesCount,

                TotalRequestedDocuments = totalRequestedDocuments,
                TotalCompletedDocuments = totalCompletedDocuments,
                TotalMissingDocuments = totalMissingDocuments,
                TotalUploadedDocuments = totalUploadedDocuments,

                OverallDocumentProgressPercentage = overallProgress,

                CasesNeedingAttention = casesNeedingAttention,
                CasesNeedingReview = casesNeedingReview,

                RecentActivity = recentActivity,

                TaxCaseStatusBreakdown = taxCaseStatusBreakdown
            };
        }

        
    }
}
