using JwtAuth.Dtos;
using JwtAuth.Dtos.Clients;
using JwtAuth.Dtos.TaxCases;

namespace JwtAuth.Services
{
    public interface IClientService
    {
        Task<PageResultDto<ClientDto>> GetClientsAsync(Guid userId, string? search, int page, int pageSize);

        Task<ClientDto?> GetClientByIdAsync(int id, Guid userId);

        Task<ClientDto> CreateClientAsync(CreateClientDto request, Guid userId);

        Task<ClientDto?> UpdateClientAsync(int id, UpdateClientDto request, Guid userId);

        Task<bool> DeleteClientAsync(int id, Guid userId);
        //Task ListClientsAsync();

        Task<bool> AssignPortalUserAsync(int clientId, Guid portalUserId, Guid adminUserId);

        Task<ClientDto?> GetClientByPortalUserIdAsync(Guid portalUserId);

        Task<List<TaxCaseDto>> GetPortalTaxCasesAsync(Guid portalUserId);

        Task<TaxCaseSummaryDto?> GetPortalTaxCaseSummaryAsync(int taxCaseId, Guid portalUserId);

        Task<bool> CreatePortalAccountAsync(int clientId, string password, Guid adminUserId);

        Task<bool> UnassignPortalUserAsync(int clientId, Guid adminUserId);

        Task<bool> ResetPortalPasswordAsync(int clientId, string password,Guid adminUserId);

        Task<ClientDto> ImportGoHighLevelContactAsync(string ghlContactId,Guid adminUserId);

        Task<ClientDto?> SyncClientToGoHighLevelAsync(int clientId, Guid adminUserId);

        Task<HashSet<string>> GetImportedGoHighLevelContactIdsAsync(Guid adminUserId);

        Task<ClientDto?> UpdateNotificationsEnabledAsync(int clientId, bool enabled, Guid adminUserId);

        Task<List<PortalUserDto>> SearchPortalUsersAsync(string email, Guid adminUserId);

        Task<PortalUserDto?> GetPortalUserByClientEmailAsync(int clientId, Guid adminUserId);
    }
}
