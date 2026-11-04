using JwtAuth.Data;
using JwtAuth.Dtos;
using JwtAuth.Dtos.Clients;
using JwtAuth.Dtos.TaxCases;
using JwtAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IGoHighLevelService _goHighLevelService;

        public ClientService(
            AppDbContext context,
            IEmailService emailService,
            IGoHighLevelService goHighLevelService
        )
        {
            _context = context;
            _emailService = emailService;
            _goHighLevelService = goHighLevelService;
        }

        // Get all clients
        public async Task<PageResultDto<ClientDto>> GetClientsAsync(Guid userId, string? search, int page, int pageSize)
        {
            var query = _context.Clients
                .Where(client => client.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(client =>
                client.FirstName.ToLower().Contains(search) ||
                client.LastName.ToLower().Contains(search) ||
                client.Email.ToLower().Contains(search));
            }
            
            int totalCount = await query.CountAsync();

            List<ClientDto> clients = await query
                .OrderBy(client => client.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(client => new ClientDto
                {
                    Id = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Email = client.Email,
                    Phone = client.Phone,
                    CreatedAt = client.CreatedAt,
                    UserId = client.UserId,
                    PortalUserId = client.PortalUserId,
                    GoHighLevelContactId = client.GoHighLevelContactId,
                    NotificationsEnabled = client.NotificationsEnabled

                })
                .ToListAsync();

           return new PageResultDto<ClientDto>
           {
               Items = clients,
               Page = page,
               PageSize = pageSize,
               TotalCount = totalCount,
               TotalPages = (int)Math.Ceiling(
                   totalCount / (double)pageSize)
           };

        }

        // Get a client by ID
        public async Task<ClientDto?> GetClientByIdAsync(int id, Guid userId)
        {
            //Client? Get the Client Object or null if not found
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client => 
                client.Id == id && 
                client.UserId == userId);

            if (client is null)
            {
                return null;
            }

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled

            };
        }

        // Create a new client
        public async Task<ClientDto> CreateClientAsync(CreateClientDto request, Guid userId)
        {
            string email = request.Email.Trim().ToLower();

           // validation for duplicate client email
            bool emailExists = await _context.Clients.AnyAsync(client =>
            client.UserId == userId &&
            client.Email == request.Email);

            if (emailExists)
            {
                throw new InvalidOperationException("A client with this email already exist.");
            }

            Client client = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email,
                Phone = request.Phone,
                UserId = userId
            };


            _context.Clients.Add(client);

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled

            };
        }

        // Update an existing client
        public async Task<ClientDto?> UpdateClientAsync(
            int id,
            UpdateClientDto request, Guid userId)
        {
            

            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == id && 
                    client.UserId == userId);

            if (client is null)
            {
                return null;
            }

            string email = request.Email.Trim().ToLower();

            bool emailExists = await _context.Clients.AnyAsync(existingClient =>
                existingClient.UserId == userId &&
                existingClient.Email == email &&
                existingClient.Id != id);

            if (emailExists)
            {
                throw new InvalidOperationException("A client with this email already exist.");
            }

            client.FirstName = request.FirstName;
            client.LastName = request.LastName;
            client.Email = email;
            client.Phone = request.Phone;

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled
            };
        }

        // Delete a client
        public async Task<bool> DeleteClientAsync(int id, Guid userId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client => 
                client.Id == id && 
                client.UserId == userId);

            if (client is null)
            {
                return false;
            }

            _context.Clients.Remove(client);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignPortalUserAsync(int clientId, Guid portalUserId, Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return false;
            }

            bool alreadyAssigned = await _context.Clients
                .AnyAsync(client =>
                    client.PortalUserId == portalUserId &&
                    client.Id != clientId);

            if (alreadyAssigned)
            {
                throw new InvalidOperationException(
                    "This portal user is already assigned to another client.");
            }

            User? portalUser = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Id == portalUserId &&
                    user.Role == "Client");

            if (portalUser is null)
            {
                throw new InvalidOperationException(
                    "Client portal user not found.");
            }

            if (
                !string.Equals(
                portalUser.Email.Trim(),
                client.Email.Trim(),
                StringComparison.OrdinalIgnoreCase
                )
            )
            {
                throw new InvalidOperationException(
                    "The portal user's email must match the client's email."
                );
            }

            client.PortalUserId = portalUser.Id;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ClientDto?> GetClientByPortalUserIdAsync(Guid portalUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.PortalUserId == portalUserId);

            if (client is null)
            {
                return null;
            }

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled
            };
        }

        public async Task<List<TaxCaseDto>> GetPortalTaxCasesAsync(Guid portalUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.PortalUserId == portalUserId);

            if (client is null)
            {
                return [];
            }

            return await _context.TaxCases
                .Where(taxCase => taxCase.ClientId == client.Id)
                .Select(taxCase => new TaxCaseDto
                {
                    Id = taxCase.Id,
                    TaxYear = taxCase.TaxYear,
                    Status = taxCase.Status,
                    CreatedAt = taxCase.CreatedAt,
                    ClientId = taxCase.ClientId,
                    RequestedDocumentsCount =
                        taxCase.DocumentRequests.Count,
                    CompletedDocumentsCount =
                        taxCase.DocumentRequests.Count(request => request.IsCompleted),
                    MissingDocumentsCount =
                        taxCase.DocumentRequests.Count(request => !request.IsCompleted),
                    DocumentProgressPercentage =
                        taxCase.DocumentRequests.Count == 0
                            ? 0
                            : (double)taxCase.DocumentRequests.Count(request => request.IsCompleted)
                              / taxCase.DocumentRequests.Count * 100
                })
                .ToListAsync();
        }

        public async Task<TaxCaseSummaryDto?> GetPortalTaxCaseSummaryAsync(int taxCaseId, Guid portalUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.PortalUserId == portalUserId);

            if (client is null)
            {
                return null;
            }

            TaxCase? taxCase = await _context.TaxCases
                .Include(taxCase => taxCase.DocumentRequests)
                    .ThenInclude(request => request.DocumentType)
                        .ThenInclude(type => type.DocumentCategory)
                .Include(taxCase => taxCase.Documents)
                    .ThenInclude(document => document.DocumentType)
                        .ThenInclude(type => type.DocumentCategory)
                .FirstOrDefaultAsync(taxCase =>
                    taxCase.Id == taxCaseId &&
                    taxCase.ClientId == client.Id);

            if (taxCase is null)
            {
                return null;
            }

            int requestedCount = taxCase.DocumentRequests.Count;
            int completedCount = taxCase.DocumentRequests.Count(request => request.IsCompleted);
            int missingCount = taxCase.DocumentRequests.Count(request => !request.IsCompleted);

            double progress =
                requestedCount == 0
                    ? 0
                    : (double)completedCount / requestedCount * 100;

            return new TaxCaseSummaryDto
            {
                Id = taxCase.Id,
                ClientId = taxCase.ClientId,
                TaxYear = taxCase.TaxYear,
                Status = taxCase.Status,
                RequestedDocumentsCount = requestedCount,
                CompletedDocumentsCount = completedCount,
                MissingDocumentsCount = missingCount,
                DocumentProgressPercentage = progress,

                MissingDocuments = taxCase.DocumentRequests
                    .Where(request => !request.IsCompleted)
                    .Select(request => new DocumentRequestSummaryDto
                    {
                        Id = request.Id,
                        DocumentTypeId = request.DocumentTypeId,
                        DocumentType = request.DocumentType.Name,
                        DocumentCategory = request.DocumentType.DocumentCategory.Name,
                        Note = request.Note
                    })
                    .ToList(),

                RecentDocuments = taxCase.Documents
                    .OrderByDescending(document => document.UploadedAt)
                    .Take(5)
                    .Select(document => new RecentDocumentDto
                    {
                        Id = document.Id,
                        FileName = document.FileName,
                        DocumentType = document.DocumentType != null
                            ? document.DocumentType.Name
                            : null,
                        DocumentCategory = document.DocumentType != null
                            ? document.DocumentType.DocumentCategory.Name
                            : null,
                        UploadedAt = document.UploadedAt
                    })
                    .ToList()
            };
        }

        public async Task<bool> CreatePortalAccountAsync(int clientId, string password, Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return false;
            }

            if (client.PortalUserId is not null)
            {
                throw new InvalidOperationException(
                    "This client already has a portal account.");
            }

            User? existingPortalUser = await _context.Users
    .FirstOrDefaultAsync(user =>
        user.Email == client.Email &&
        user.Role == "Client");

            if (existingPortalUser is not null)
            {
                bool alreadyAssigned = await _context.Clients
                    .AnyAsync(existingClient =>
                        existingClient.PortalUserId == existingPortalUser.Id &&
                        existingClient.Id != client.Id);

                if (alreadyAssigned)
                {
                    throw new InvalidOperationException(
                        "A portal account with this email is already assigned to another client."
                    );
                }

                client.PortalUserId = existingPortalUser.Id;

                existingPortalUser.PasswordHash =
                    new PasswordHasher<User>()
                        .HashPassword(existingPortalUser, password);

                existingPortalUser.RefreshToken = null;
                existingPortalUser.RefreshTokenExpiryTime = null;

                await _context.SaveChangesAsync();

                if (
                    client.NotificationsEnabled &&
                    !string.IsNullOrWhiteSpace(client.GoHighLevelContactId)
                )
                {
                    await _emailService.SendPortalAccountCreatedAsync(
                        client.GoHighLevelContactId,
                        client.Email,
                        password
                    );
                }

                return true;
            }

            var user = new User
            {
                Email = client.Email,
                Role = "Client"
            };

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, password);

            _context.Users.Add(user);

            client.PortalUserId = user.Id;

            await _context.SaveChangesAsync();

            if (client.NotificationsEnabled && !string.IsNullOrWhiteSpace(client.GoHighLevelContactId))
            {
                await _emailService.SendPortalAccountCreatedAsync(
                    client.GoHighLevelContactId,
                    client.Email,
                    password
                );
            }
            return true;
        }

        public async Task<bool> UnassignPortalUserAsync(int clientId, Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return false;
            }

            client.PortalUserId = null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPortalPasswordAsync(int clientId, string password, Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return false;
            }

            if (client.PortalUserId is null)
            {
                throw new InvalidOperationException(
                    "This client does not have a portal account.");
            }

            User? portalUser = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Id == client.PortalUserId &&
                    user.Role == "Client");

            if (portalUser is null)
            {
                throw new InvalidOperationException(
                    "Client portal user not found.");
            }

            portalUser.PasswordHash = new PasswordHasher<User>()
                .HashPassword(portalUser, password);

            portalUser.RefreshToken = null;
            portalUser.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            //Call the reset-email method from existing client
            if (client.NotificationsEnabled && !string.IsNullOrWhiteSpace(client.GoHighLevelContactId))
            {
                await _emailService.SendPortalPasswordResetAsync(
                    client.GoHighLevelContactId,
                    portalUser.Email,
                    password
                );
            }

            return true;
        }

        public async Task<ClientDto> ImportGoHighLevelContactAsync(
    string ghlContactId,
    Guid adminUserId
)
        {
            GoHighLevelContactDto? ghlContact =
                await _goHighLevelService.GetContactAsync(ghlContactId);

            if (ghlContact is null)
            {
                throw new InvalidOperationException(
                    "GoHighLevel contact was not found."
                );
            }

            bool ghlContactAlreadyImported =
                await _context.Clients.AnyAsync(client =>
                    client.UserId == adminUserId &&
                    client.GoHighLevelContactId == ghlContactId);

            if (ghlContactAlreadyImported)
            {
                throw new InvalidOperationException(
                    "This GoHighLevel contact has already been imported."
                );
            }

            string? email = ghlContact.Email?
                .Trim()
                .ToLower();

            if (!string.IsNullOrWhiteSpace(email))
            {
                bool emailExists =
                    await _context.Clients.AnyAsync(client =>
                        client.UserId == adminUserId &&
                        client.Email == email);

                if (emailExists)
                {
                    throw new InvalidOperationException(
                        "A ClientFlow client with this email already exists."
                    );
                }
            }

            Client client = new Client
            {
                FirstName = ghlContact.FirstName,
                LastName = ghlContact.LastName,
                Email = email ?? string.Empty,
                Phone = ghlContact.Phone,
                UserId = adminUserId,
                GoHighLevelContactId = ghlContact.Id
            };

            _context.Clients.Add(client);

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled,
            };
        }

        public async Task<ClientDto?> SyncClientToGoHighLevelAsync(
    int clientId,
    Guid adminUserId
)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(client.GoHighLevelContactId))
            {
                throw new InvalidOperationException(
                    "This client is already linked to GoHighLevel."
                );
            }

            if (string.IsNullOrWhiteSpace(client.Email))
            {
                throw new InvalidOperationException(
                    "The client must have an email address before syncing to GoHighLevel."
                );
            }

            string? ghlContactId =
                await _goHighLevelService.UpsertContactAsync(
                    client.FirstName,
                    client.LastName,
                    client.Email,
                    client.Phone
                );

            if (string.IsNullOrWhiteSpace(ghlContactId))
            {
                throw new InvalidOperationException(
                    "GoHighLevel did not return a contact ID."
                );
            }

            client.GoHighLevelContactId = ghlContactId;

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled,
            };
        }

        public async Task<HashSet<string>> GetImportedGoHighLevelContactIdsAsync(Guid adminUserId)
        {
            var contactIds = await _context.Clients
                .Where(client =>
                    client.UserId == adminUserId &&
                    client.GoHighLevelContactId != null)
                .Select(client => client.GoHighLevelContactId!)
                .ToListAsync();

            return contactIds.ToHashSet();
        }

        public async Task<ClientDto?> UpdateNotificationsEnabledAsync(
    int clientId,
    bool enabled,
    Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return null;
            }

            if (enabled &&
                string.IsNullOrWhiteSpace(client.GoHighLevelContactId))
            {
                throw new InvalidOperationException(
                    "Link this client to GoHighLevel before enabling notifications."
                );
            }

            client.NotificationsEnabled = enabled;

            await _context.SaveChangesAsync();

            return new ClientDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                CreatedAt = client.CreatedAt,
                UserId = client.UserId,
                PortalUserId = client.PortalUserId,
                GoHighLevelContactId = client.GoHighLevelContactId,
                NotificationsEnabled = client.NotificationsEnabled
            };
        }

        public async Task<List<PortalUserDto>> SearchPortalUsersAsync(
    string email,
    Guid adminUserId)
        {
            email = email.Trim().ToLower();

            var users = await _context.Users
                .Where(user =>
                    user.Role == "Client" &&
                    user.Email.ToLower().Contains(email))
                .Select(user => new PortalUserDto
                {
                    Id = user.Id,
                    Email = user.Email,

                    IsAssigned = _context.Clients.Any(client =>
                        client.PortalUserId == user.Id &&
                        client.UserId == adminUserId)
                })
                .Take(10)
                .ToListAsync();

            return users;
        }

        public async Task<PortalUserDto?> GetPortalUserByClientEmailAsync(
    int clientId,
    Guid adminUserId)
        {
            Client? client = await _context.Clients
                .FirstOrDefaultAsync(client =>
                    client.Id == clientId &&
                    client.UserId == adminUserId);

            if (client is null)
            {
                return null;
            }

            string email = client.Email.Trim().ToLower();

            User? portalUser = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Role == "Client" &&
                    user.Email.ToLower() == email);

            if (portalUser is null)
            {
                return null;
            }

            bool isAssigned = await _context.Clients
                .AnyAsync(client =>
                    client.PortalUserId == portalUser.Id);

            return new PortalUserDto
            {
                Id = portalUser.Id,
                Email = portalUser.Email,
                IsAssigned = isAssigned
            };
        }
    }
}
