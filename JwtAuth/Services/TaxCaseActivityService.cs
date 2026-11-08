using JwtAuth.Data;
using JwtAuth.Dtos.TaxCases;
using JwtAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class TaxCaseActivityService : ITaxCaseActivityService
    {
        private readonly AppDbContext _context;

        public TaxCaseActivityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaxCaseActivityDto>> GetActivitiesAsync(
            int taxCaseId,
            Guid userId)
        {
            bool hasAccess = await _context.TaxCases.AnyAsync(taxCase =>
                taxCase.Id == taxCaseId &&
                taxCase.Client.UserId == userId);

            if (!hasAccess)
            {
                return [];
            }

            return await _context.TaxCaseActivities
                .Where(activity =>
                    activity.TaxCaseId == taxCaseId)
                .OrderByDescending(activity =>
                    activity.CreatedAt)
                .Select(activity => new TaxCaseActivityDto
                {
                    Id = activity.Id,
                    TaxCaseId = activity.TaxCaseId,
                    Action = activity.Action,
                    Description = activity.Description,

                    UserName =
                        activity.User.FirstName + " " +
                        activity.User.LastName,

                    CreatedAt = activity.CreatedAt
                })
                .ToListAsync();
        }

        public async Task CreateActivityAsync(
            int taxCaseId,
            Guid userId,
            string action,
            string? description)
        {
            TaxCaseActivity activity = new()
            {
                TaxCaseId = taxCaseId,
                UserId = userId,
                Action = action,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaxCaseActivities.Add(activity);

            await _context.SaveChangesAsync();
        }
    }
}