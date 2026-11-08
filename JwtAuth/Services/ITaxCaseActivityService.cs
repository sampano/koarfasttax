using JwtAuth.Dtos.TaxCases;

namespace JwtAuth.Services
{
    public interface ITaxCaseActivityService
    {
        Task<List<TaxCaseActivityDto>> GetActivitiesAsync(
            int taxCaseId,
            Guid userId);

        Task CreateActivityAsync(
            int taxCaseId,
            Guid userId,
            string action,
            string? description);
    }
}