using JwtAuth.Data;
using JwtAuth.Dtos.DocumentRequests;
using JwtAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class DocumentRequestService : IDocumentRequestService
    {
        private readonly AppDbContext _context;
        private readonly IClientNotificationService _clientNotificationService;
        private readonly INotificationService _notificationService;

        public DocumentRequestService(AppDbContext context, IClientNotificationService clientNotificationService, INotificationService notificationService)
        {
            _context = context;
            _clientNotificationService = clientNotificationService;
            _notificationService = notificationService;
        }
        public async Task<DocumentRequestDto?> CreateDocumentRequestAsync(
            int taxCaseId,
            CreateDocumentRequestDto request,
            Guid userId)
        {
            TaxCase? taxCase = await _context.TaxCases
                .Include(taxCase => taxCase.Client)
                .FirstOrDefaultAsync(taxCase =>
                    taxCase.Id == taxCaseId &&
                    taxCase.Client.UserId == userId);

            if (taxCase is null)
            {
                return null;
            }

            DocumentType? documentType = await _context.DocumentTypes
                    .Include(documentType =>
                        documentType.DocumentCategory)
                    .FirstOrDefaultAsync(documentType =>
                        documentType.Id == request.DocumentTypeId);

            if (documentType is null)
            {
                throw new InvalidOperationException(
                    "The selected document type does not exist.");
            }

            bool requestExists = await _context.DocumentRequests.AnyAsync(
                    documentRequest =>
                        documentRequest.TaxCaseId == taxCaseId &&
                        documentRequest.DocumentTypeId ==
                            request.DocumentTypeId);

            if (requestExists)
            {
                throw new InvalidOperationException(
                    "This document has already been requested.");
            }

            DocumentRequest documentRequest = new DocumentRequest
            {
                TaxCaseId = taxCaseId,
                DocumentTypeId = request.DocumentTypeId,
                Note = request.Note,
                Status = DocumentRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.DocumentRequests.Add(documentRequest);

            await _context.SaveChangesAsync();

            if (
                 taxCase.Client.NotificationsEnabled &&
                 !string.IsNullOrWhiteSpace(taxCase.Client.GoHighLevelContactId)
             )
            {
                string subject =
                    $"New document requested for your {taxCase.TaxYear} tax case";

                string html = $"""
                <h2>New document requested</h2>

                <p>
                    A new document has been requested for your
                    {taxCase.TaxYear} Koar Fast Tax tax case.
                </p>

                <p>
                    <strong>Document:</strong> {documentType.Name}
                </p>

                {(string.IsNullOrWhiteSpace(request.Note)
                                ? ""
                                : $"<p><strong>Note:</strong> {request.Note}</p>")}

                <p>
                    Please log in to your Koar Fast Tax client portal
                    to review the request and upload the document.
                </p>
                """;

                        string smsMessage =
                            $"Koar Fast Tax: {documentType.Name} was requested for your " +
                            $"{taxCase.TaxYear} tax case. Please log in to your portal.";

                        await _clientNotificationService.SendAsync(
                            taxCase.Client.GoHighLevelContactId,
                            taxCase.Client.Phone,
                            subject,
                            html,
                            smsMessage
                        );
            }
            if (taxCase.Client.PortalUserId is not null)
            {
                await _notificationService.CreateNotificationAsync(
                    taxCase.Client.PortalUserId.Value,
                    "New document requested",
                    $"{documentType.Name} was requested for your {taxCase.TaxYear} tax case.",
                    $"/client/tax-cases/{taxCase.Id}"
                );
            }

            return new DocumentRequestDto
            {
                Id = documentRequest.Id,
                TaxCaseId = documentRequest.TaxCaseId,
                DocumentTypeId = documentRequest.DocumentTypeId,
                Note = documentRequest.Note,
                DocumentType = documentType.Name,
                DocumentCategory = documentType.DocumentCategory.Name,
                Status = documentRequest.Status,
                CreatedAt = documentRequest.CreatedAt
            };
        }

        public async Task<List<DocumentRequestDto>> GetDocumentRequestsAsync(int taxCaseId, Guid userId)
        {
            return await _context.DocumentRequests
                .Where(documentRequest =>
                    documentRequest.TaxCaseId == taxCaseId &&
                    documentRequest.TaxCase.Client.UserId == userId)
                .OrderBy(documentRequest =>
                    documentRequest.DocumentType.DocumentCategory.Name)
                .ThenBy(documentRequest =>
                    documentRequest.DocumentType.Name)
                .Select(documentRequest =>
                    new DocumentRequestDto
                    {
                        Id = documentRequest.Id,
                        TaxCaseId = documentRequest.TaxCaseId,
                        DocumentTypeId = documentRequest.DocumentTypeId,
                        DocumentType = documentRequest.DocumentType.Name,
                        DocumentCategory = documentRequest.DocumentType.DocumentCategory.Name,
                        Note = documentRequest.Note,
                        Status = documentRequest.Status,
                        CreatedAt = documentRequest.CreatedAt
                    })
                .ToListAsync();
        }

        public async Task<bool> DeleteDocumentRequestAsync(int documentRequestId, Guid userId)
        {
            DocumentRequest? documentRequest = await _context.DocumentRequests
                    .FirstOrDefaultAsync(documentRequest =>
                        documentRequest.Id == documentRequestId &&
                        documentRequest.TaxCase.Client.UserId == userId);

            if (documentRequest is null)
            {
                return false;
            }

            _context.DocumentRequests.Remove(documentRequest);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DocumentRequestDto?> UpdateDocumentRequestAsync(int documentRequestId, UpdateDocumentRequestDto request, Guid userId)
        {
            DocumentRequest? documentRequest =await _context.DocumentRequests
                    .Include(documentRequest => documentRequest.DocumentType)
                        .ThenInclude(documentType => documentType.DocumentCategory)
                    .FirstOrDefaultAsync(documentRequest =>
                        documentRequest.Id == documentRequestId &&
                        documentRequest.TaxCase.Client.UserId == userId);

            if (documentRequest is null)
            {
                return null;
            }

            documentRequest.Note = request.Note;

            await _context.SaveChangesAsync();

            return new DocumentRequestDto
            {
                Id = documentRequest.Id,
                TaxCaseId = documentRequest.TaxCaseId,
                DocumentTypeId = documentRequest.DocumentTypeId,
                DocumentType = documentRequest.DocumentType.Name,
                DocumentCategory = documentRequest.DocumentType.DocumentCategory.Name,
                Note = documentRequest.Note,
                Status = documentRequest.Status,
                CreatedAt = documentRequest.CreatedAt
            };
        }

        public async Task<DocumentRequestDto?> CompleteDocumentRequestAsync(
    int documentRequestId,
    Guid userId)
        {
            DocumentRequest? documentRequest =
                await _context.DocumentRequests
                    .Include(request => request.DocumentType)
                        .ThenInclude(type => type.DocumentCategory)
                    .FirstOrDefaultAsync(request =>
                        request.Id == documentRequestId &&
                        request.TaxCase.Client.UserId == userId);

            if (documentRequest is null)
            {
                return null;
            }

            if (documentRequest.Status != DocumentRequestStatus.Uploaded)
            {
                throw new InvalidOperationException(
                    "Only uploaded document requests can be marked as completed.");
            }

            documentRequest.Status = DocumentRequestStatus.Completed;

            await _context.SaveChangesAsync();

            return new DocumentRequestDto
            {
                Id = documentRequest.Id,
                TaxCaseId = documentRequest.TaxCaseId,
                DocumentTypeId = documentRequest.DocumentTypeId,
                DocumentType = documentRequest.DocumentType.Name,
                DocumentCategory =
                    documentRequest.DocumentType.DocumentCategory.Name,
                Note = documentRequest.Note,
                Status = documentRequest.Status,
                CreatedAt = documentRequest.CreatedAt
            };
        }

    }
}
