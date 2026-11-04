namespace JwtAuth.Services
{
    public class ClientNotificationService : IClientNotificationService
    {
        private readonly IGoHighLevelService _goHighLevelService;

        public ClientNotificationService(
            IGoHighLevelService goHighLevelService)
        {
            _goHighLevelService = goHighLevelService;
        }

        public async Task SendAsync(
            string contactId,
            string? phone,
            string emailSubject,
            string emailHtml,
            string smsMessage)
        {
            // Email can still be sent even when the client has no phone.
            await _goHighLevelService.SendEmailAsync(
                contactId,
                emailSubject,
                emailHtml
            );

            // Only send SMS when a phone number exists.
            if (!string.IsNullOrWhiteSpace(phone))
            {
                await _goHighLevelService.SendSmsAsync(
                    contactId,
                    smsMessage
                );
            }
        }
    }
}