using JwtAuth.Data;
using JwtAuth.Dtos.Documents;
using JwtAuth.Entities;
using JwtAuth.Enums;
using Microsoft.EntityFrameworkCore;


namespace JwtAuth.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IClientNotificationService _clientNotificationService;
        private readonly INotificationService _notificationService;

        private readonly ITaxCaseActivityService _taxCaseActivityService;
        public DocumentService(
       AppDbContext context,
       IWebHostEnvironment environment,
       IClientNotificationService clientNotificationService,
       INotificationService notificationService,
       ITaxCaseActivityService taxCaseActivityService)
        {
            _context = context;
            _environment = environment;
            _clientNotificationService = clientNotificationService;
            _notificationService = notificationService;
            _taxCaseActivityService = taxCaseActivityService;
        }

        public async Task<DocumentDto?> UploadDocumentAsync(int taxCaseId, IFormFile file, int documentTypeId, Guid userId)
            {
                TaxCase? taxCase = await _context.TaxCases
                .FirstOrDefaultAsync(taxCase =>
                taxCase.Id == taxCaseId &&
                taxCase.Client.UserId == userId);

                if (taxCase is null)
                {
                    return null;
                }

            // Check if the document type exists
                DocumentType? documentType =
                    await _context.DocumentTypes
                        .Include(documentType => documentType.DocumentCategory)
                        .FirstOrDefaultAsync(documentType =>
                            documentType.Id == documentTypeId);

                if (documentType is null)
                {
                    throw new InvalidOperationException(
                        "The selected document type does not exist.");
                }

                if (file.Length == 0)
                {
                    throw new InvalidOperationException("The uploaded file is empty");
                }

                const long maxFileSize = 10 * 1024 * 1024;

                if(file.Length > maxFileSize)
                {
                    throw new InvalidOperationException("The file size cannot exceed 10 MB");
                }

                string[] allowedExtensions =
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                string extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Only PDF, JPG, and PNG files are allowed.");
                }

                string uploadsFolder = Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads");

                Directory.CreateDirectory(uploadsFolder);

                string storedFileName = $"{Guid.NewGuid()}{extension}";

                string filePath = Path.Combine(uploadsFolder, storedFileName);

                using (FileStream stream = new FileStream(filePath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Document document = new Document
                {
                    FileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow,
                    TaxCaseId = taxCaseId,
                    UploadedByUserId = userId,
                    DocumentTypeId = documentTypeId
                };
                
                _context.Documents.Add(document);

                 await _context.SaveChangesAsync();

                DocumentRequest? documentRequest =
                await _context.DocumentRequests
                    .FirstOrDefaultAsync(request =>
                        request.TaxCaseId == taxCaseId &&
                        request.DocumentTypeId == documentTypeId);

                if (documentRequest is not null)
                {
                    documentRequest.Status = DocumentRequestStatus.Uploaded;

                    await _context.SaveChangesAsync();

                    await _taxCaseActivityService.CreateActivityAsync(
                        documentRequest.TaxCaseId,
                        userId,
                        "DocumentUploaded",
                        $"{documentType.Name} was uploaded.");
                }

            return new DocumentDto
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    TaxCaseId = taxCaseId,
                    DocumentTypeId = document.DocumentTypeId,
                    DocumentType = documentType.Name,
                    DocumentCategory = documentType.DocumentCategory.Name
                };
            
        }
        
        public async Task<List<DocumentDto>> GetDocumentByTaxCaseAsync(int taxCaseId, Guid userId)
        {
            return await _context.Documents
                .Where(document =>
                    document.TaxCaseId == taxCaseId &&
                    document.TaxCase.Client.UserId == userId)
                .OrderByDescending(document => document.UploadedAt)
                .Select(document => new DocumentDto
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedAt= document.UploadedAt,
                    TaxCaseId = document.TaxCaseId,
                    DocumentTypeId = document.DocumentTypeId,
                    DocumentType = document.DocumentType != null
                        ? document.DocumentType.Name
                        : null,
                    DocumentCategory = document.DocumentType != null
                        ? document.DocumentType.DocumentCategory.Name
                        : null
                })
                .ToListAsync();
        }


        public async Task<DownloadDocumentDto?> GetDocumentForDownloadAsync(int documenId, Guid userId)
        {
            Document? document = await _context.Documents
                .FirstOrDefaultAsync(document =>
                    document.Id == documenId &&
                    document.TaxCase.Client.UserId == userId);

            if (document is null)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");

            string filePath = Path.Combine(uploadsFolder, document.StoredFileName);

            if (!File.Exists(filePath))
            {
                return null;
            }


            return new DownloadDocumentDto
            {
                FilePath = filePath,
                FileName = document.FileName,
                ContentType = document.ContentType
            };
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, Guid userId)
        {
            Document? document = await _context.Documents
            .Include(document => document.DocumentType)
            .FirstOrDefaultAsync(document =>
                document.Id == documentId &&
                document.TaxCase.Client.UserId == userId);

            string documentTypeName = document.DocumentType?.Name ?? "Document";

            if (document is null)
            {
                return false;
            }

            int taxCaseId = document.TaxCaseId;

            int? documentTypeId = document.DocumentTypeId;

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            string filePath = Path.Combine(
                uploadsFolder,
                document.StoredFileName);

            //Deletes physical file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Delete  record in DB
            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();


            // Check if another same type document still exist
            bool returnedToPending = false;

            if (documentTypeId is not null)
            {
                bool anotherDocumentExists =
                    await _context.Documents.AnyAsync(existingDocument =>
                        existingDocument.TaxCaseId == taxCaseId &&
                        existingDocument.DocumentTypeId == documentTypeId);

                if (!anotherDocumentExists)
                {
                    DocumentRequest? documentRequest =
                        await _context.DocumentRequests
                            .FirstOrDefaultAsync(request =>
                                request.TaxCaseId == taxCaseId &&
                                request.DocumentTypeId == documentTypeId);

                    if (documentRequest is not null)
                    {
                        documentRequest.Status = DocumentRequestStatus.Pending;

                        await _context.SaveChangesAsync();

                        returnedToPending = true;
                    }
                }
            }

            await _taxCaseActivityService.CreateActivityAsync(
                taxCaseId,
                userId,
                "DocumentDeleted",
                returnedToPending
                    ? $"{documentTypeName} was deleted and returned to Pending."
                    : $"{documentTypeName} was deleted.");

            return true;

        }

        public async Task<List<DocumentCategoryDto>> GetDocumentCategoriesAsync()
        {
            return await _context.DocumentCategories
                .OrderBy(category => category.Name)
                .Select(category => new DocumentCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    DocumentTypes = category.DocumentTypes
                        .OrderBy(documentType => documentType.Name)
                        .Select(documentType => new DocumentTypeDto
                        {
                            Id = documentType.Id,
                            Name = documentType.Name
                        })
                .ToList()
                }).ToListAsync();
        }

        public async Task<DocumentDto?> UploadPortalDocumentAsync(int taxCaseId, IFormFile file, int documentTypeId, Guid portalUserId)
        {
            TaxCase? taxCase = await _context.TaxCases
                .Include(taxCase => taxCase.Client)
                .ThenInclude(client => client.User)
                .FirstOrDefaultAsync(taxCase =>
                    taxCase.Id == taxCaseId &&
                    taxCase.Client.PortalUserId == portalUserId);

            if (taxCase is null)
            {
                return null;
            }

            DocumentType? documentType =
            await _context.DocumentTypes
                .Include(documentType => documentType.DocumentCategory)
                .FirstOrDefaultAsync(documentType =>
                    documentType.Id == documentTypeId);

            if (documentType is null)
            {
                throw new InvalidOperationException(
                    "The selected document type does not exist.");
            }

            if (file.Length == 0)
            {
                throw new InvalidOperationException(
                    "The uploaded file is empty.");
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException(
                    "The file size cannot exceed 10 MB.");
            }

            string[] allowedExtensions =
            {
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png"
            };

            string extension =
                Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Only PDF, JPG, and PNG files are allowed.");
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            Directory.CreateDirectory(uploadsFolder);

            string storedFileName =
                $"{Guid.NewGuid()}{extension}";

            string filePath =
                Path.Combine(uploadsFolder, storedFileName);

            using (FileStream stream =
                new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Document document = new Document
            {
                FileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                TaxCaseId = taxCaseId,
                UploadedByUserId = portalUserId,
                DocumentTypeId = documentTypeId
            };

            _context.Documents.Add(document);

            await _context.SaveChangesAsync();

            DocumentRequest? documentRequest =
                await _context.DocumentRequests
                    .FirstOrDefaultAsync(request =>
                        request.TaxCaseId == taxCaseId &&
                        request.DocumentTypeId == documentTypeId);

            if (documentRequest is not null)
            {
                documentRequest.Status = DocumentRequestStatus.Uploaded;

                await _context.SaveChangesAsync();

                await _taxCaseActivityService.CreateActivityAsync(
                    documentRequest.TaxCaseId,
                    portalUserId,
                    "DocumentUploaded",
                    $"{documentType.Name} was uploaded.");
            }

            bool hasDocumentRequests = await _context.DocumentRequests.AnyAsync(request =>
                request.TaxCaseId == taxCaseId);

            bool hasPendingRequests =
                 await _context.DocumentRequests.AnyAsync(request =>
                     request.TaxCaseId == taxCaseId &&
                     request.Status == DocumentRequestStatus.Pending);

            if (hasDocumentRequests && !hasPendingRequests)
            {
                if (
                    taxCase.Client.NotificationsEnabled &&
                    !string.IsNullOrWhiteSpace(
                        taxCase.Client.GoHighLevelContactId
                    )
                )
                {
                    string subject =
                        $"All documents received for your {taxCase.TaxYear} tax case";

                    string html = $"""
            <h2>All requested documents have been received</h2>

            <p>
                We have received all requested documents for your
                {taxCase.TaxYear} Koar Fast Tax tax case.
            </p>

            <p>
                Your tax professional can now continue reviewing your case.
            </p>

            <p>
                You can log in to your Koar Fast Tax client portal
                to check the current status of your tax case.
            </p>
            """;

                    string smsMessage =
                        $"Koar Fast Tax: All requested documents for your " +
                        $"{taxCase.TaxYear} tax case have been received.";

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
                        "All documents received",
                        $"All requested documents for your {taxCase.TaxYear} tax case have been received.",
                        $"/client/tax-cases/{taxCase.Id}"
                    );
                }
            }


               
                string clientName = $"{taxCase.Client.FirstName} {taxCase.Client.LastName}";

                await _notificationService.CreateNotificationAsync(
                    taxCase.Client.UserId,
                    "New document uploaded",
                    $"{clientName} uploaded {documentType.Name} for tax year {taxCase.TaxYear}.",
                    $"/tax-cases/{taxCase.Id}"
                );

                if (
                    taxCase.Client.NotificationsEnabled &&
                    !string.IsNullOrWhiteSpace(
                        taxCase.Client.GoHighLevelContactId
                    )
                )
                {
                    string subject =
                        $"Document received for your {taxCase.TaxYear} tax case";

                    string html = $"""
                    <h2>Your document was received</h2>

                    <p>
                        We successfully received your document for your
                        {taxCase.TaxYear} Koar Fast Tax tax case.
                    </p>

                    <p>
                        <strong>Document:</strong> {documentType.Name}
                    </p>

                    <p>
                        No further action is required for this document
                        unless your tax professional contacts you.
                    </p>
                    """;

                    string smsMessage =
                        $"Koar Fast Tax: We received your {documentType.Name} " +
                        $"for your {taxCase.TaxYear} tax case.";

                    await _clientNotificationService.SendAsync(
                        taxCase.Client.GoHighLevelContactId,
                        taxCase.Client.Phone,
                        subject,
                        html,
                        smsMessage
                    );
                }
     
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt,
                TaxCaseId = document.TaxCaseId,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = documentType?.Name,
                DocumentCategory = documentType?.DocumentCategory.Name
            };


        }

        public async Task<List<DocumentDto>> GetPortalDocumentsByTaxCaseAsync(int taxCaseId, Guid portalUserId)
        {
            return await _context.Documents
                .Where(document =>
                    document.TaxCaseId == taxCaseId &&
                    document.TaxCase.Client.PortalUserId
                        == portalUserId)
                .OrderByDescending(document =>
                    document.UploadedAt)
                .Select(document => new DocumentDto
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    TaxCaseId = document.TaxCaseId,
                    DocumentTypeId = document.DocumentTypeId,
                    DocumentType =
                        document.DocumentType != null
                            ? document.DocumentType.Name
                            : null,
                    DocumentCategory =
                        document.DocumentType != null
                            ? document.DocumentType
                                .DocumentCategory.Name
                            : null
                })
                .ToListAsync();
        }

        public async Task<DownloadDocumentDto?> GetPortalDocumentForDownloadAsync(int documentId, Guid portalUserId)
        {
            Document? document =
                await _context.Documents
                    .FirstOrDefaultAsync(document =>
                        document.Id == documentId &&
                        document.TaxCase.Client.PortalUserId
                            == portalUserId);

            if (document is null)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            string filePath = Path.Combine(
                uploadsFolder,
                document.StoredFileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return new DownloadDocumentDto
            {
                FilePath = filePath,
                FileName = document.FileName,
                ContentType = document.ContentType
            };
        }

        public async Task<bool> DeletePortalDocumentAsync(int documentId, Guid portalUserId)
        {
            Document? document = await _context.Documents
                .Include(document => document.DocumentType)
                .Include(document => document.TaxCase)
                    .ThenInclude(taxCase => taxCase.Client)
                .FirstOrDefaultAsync(document =>
                    document.Id == documentId &&
                    document.TaxCase.Client.PortalUserId == portalUserId &&
                    document.UploadedByUserId == portalUserId);

            string documentTypeName =
                document.DocumentType?.Name ?? "Document";

            bool returnedToPending = false;

            if (document is null)
            {
                return false;
            }

            if (document.TaxCase.Status == TaxCaseStatus.Filed ||
                document.TaxCase.Status == TaxCaseStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Documents cannot be deleted after the tax case has been filed or completed.");
            }

            int taxCaseId = document.TaxCaseId;
            int? documentTypeId = document.DocumentTypeId;

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            string filePath = Path.Combine(
                uploadsFolder,
                document.StoredFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();

            if (documentTypeId is not null)
            {
                bool anotherDocumentExists =
                    await _context.Documents.AnyAsync(existingDocument =>
                        existingDocument.TaxCaseId == taxCaseId &&
                        existingDocument.DocumentTypeId == documentTypeId);

                if (!anotherDocumentExists)
                {
                    DocumentRequest? documentRequest =
                        await _context.DocumentRequests
                            .FirstOrDefaultAsync(request =>
                                request.TaxCaseId == taxCaseId &&
                                request.DocumentTypeId == documentTypeId);

                    if (documentRequest is not null)
                    {
                        documentRequest.Status = DocumentRequestStatus.Pending;

                        await _context.SaveChangesAsync();
                    }
                }
            }

            await _taxCaseActivityService.CreateActivityAsync(
                taxCaseId,
                portalUserId,
                "DocumentDeleted",
                returnedToPending
                    ? $"{documentTypeName} was deleted and returned to Pending."
                    : $"{documentTypeName} was deleted.");

            return true;
        }
    }
}