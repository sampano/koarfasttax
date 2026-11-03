using JwtAuth.Dtos;

namespace JwtAuth.Services
{
    public interface IGoHighLevelService
    {
        Task<string?> UpsertContactAsync(
            string firstName,
            string lastName,
            string email,
            string? phone
        );
        Task<List<GoHighLevelContactDto>> SearchContactsAsync(string keyword);

        Task<GoHighLevelContactDto?> GetContactAsync(string contactId);

        Task<string?> SendEmailAsync(string contactId,string subject,string html);

        Task<string?> SendSmsAsync(string contactId,string message);
    }
}