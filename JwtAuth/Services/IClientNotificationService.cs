namespace JwtAuth.Services
{
    public interface IClientNotificationService
    {
        Task SendAsync(
     string contactId,
     string? phone,
     string emailSubject,
     string emailHtml,
     string smsMessage
 );
    }
}