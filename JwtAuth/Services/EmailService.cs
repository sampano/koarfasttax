namespace JwtAuth.Services
{
    public class EmailService : IEmailService
    {
        private readonly IGoHighLevelService _goHighLevelService;

        public EmailService(
            IGoHighLevelService goHighLevelService)
        {
            _goHighLevelService = goHighLevelService;
        }

        public async Task SendPortalAccountCreatedAsync(
            string contactId,
            string email,
            string temporaryPassword)
        {
            string subject = "Your Koar Fast Tax Portal Account";

            string html = $"""
                <h2>Your portal account is ready</h2>

                <p>
                    Your Koar Fast Tax client portal account has been created.
                </p>

                <p>
                    <strong>Email:</strong> {email}<br />
                    <strong>Temporary Password:</strong> {temporaryPassword}
                </p>

                <p>
                    Please log in and keep your account information secure.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendPortalPasswordResetAsync(
            string contactId,
            string email,
            string temporaryPassword)
        {
            string subject =
                "Your Koar Fast Tax Password Reset Is Complete";

            string html = $"""
                <h2>Your password reset is complete</h2>

                <p>
                    Your Koar Fast Tax client portal password has been
                    successfully reset.
                </p>

                <p>
                    <strong>Email:</strong> {email}<br />
                    <strong>New Temporary Password:</strong>
                    {temporaryPassword}
                </p>

                <p>
                    You can now log in using your new temporary password.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendDocumentRequestCreatedAsync(
            string contactId,
            int taxYear,
            string documentType,
            string? note)
        {
            string subject =
                $"New document requested for your {taxYear} tax case";

            string html = $"""
                <h2>New document requested</h2>

                <p>
                    A new document has been requested for your
                    {taxYear} Koar Fast Tax tax case.
                </p>

                <p>
                    <strong>Document:</strong> {documentType}
                </p>

                {(string.IsNullOrWhiteSpace(note)
                    ? ""
                    : $"<p><strong>Note:</strong> {note}</p>")}

                <p>
                    Please log in to your Koar Fast Tax client portal
                    to review the request and upload the document.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendTaxCaseStatusChangedAsync(
            string contactId,
            int taxYear,
            string status)
        {
            string subject =
                $"Your {taxYear} Tax Case Status Has Been Updated";

            string html = $"""
                <h2>Your tax case status has been updated</h2>

                <p>
                    The status of your {taxYear}
                    Koar Fast Tax tax case has changed.
                </p>

                <p>
                    <strong>Current Status:</strong> {status}
                </p>

                <p>
                    Please log in to your Koar Fast Tax client portal
                    to review your case.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendDocumentUploadedAsync(
            string contactId,
            int taxYear,
            string documentType)
        {
            string subject =
                $"Document received for your {taxYear} tax case";

            string html = $"""
                <h2>Your document was received</h2>

                <p>
                    We successfully received your document for your
                    {taxYear} Koar Fast Tax tax case.
                </p>

                <p>
                    <strong>Document:</strong> {documentType}
                </p>

                <p>
                    No further action is required for this document
                    unless your tax professional contacts you.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendAllDocumentsCompletedAsync(
            string contactId,
            int taxYear)
        {
            string subject =
                $"All documents received for your {taxYear} tax case";

            string html = $"""
                <h2>All requested documents have been received</h2>

                <p>
                    We have received all requested documents for your
                    {taxYear} Koar Fast Tax tax case.
                </p>

                <p>
                    Your tax professional can now continue reviewing
                    your case.
                </p>

                <p>
                    You can log in to your Koar Fast Tax client portal
                    to check the current status of your tax case.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendAdminDocumentUploadedAsync(
            string contactId,
            string clientName,
            int taxYear,
            string documentType,
            string fileName)
        {
            string subject =
                $"{clientName} uploaded a document";

            string html = $"""
                <h2>New client document uploaded</h2>

                <p>
                    {clientName} uploaded a new document
                    for their {taxYear} tax case.
                </p>

                <p>
                    <strong>Document:</strong> {documentType}<br />
                    <strong>File:</strong> {fileName}
                </p>

                <p>
                    Log in to Koar Fast Tax to review
                    the uploaded document.
                </p>
                """;

            await _goHighLevelService.SendEmailAsync(
                contactId,
                subject,
                html
            );
        }

        public async Task SendForgotPasswordAsync(
            string contactId,
            string email,
            string resetUrl)
            {
                string subject =
                    "Reset Your Koar Fast Tax Portal Password";

                string html = $"""
                <h2>Reset your password</h2>

                <p>
                    We received a request to reset the password
                    for your Koar Fast Tax client portal account.
                </p>

                <p>
                    <strong>Account:</strong> {email}
                </p>

                <p>
                    Click the link below to create a new password:
                </p>

                <p>
                    <a href="{resetUrl}">
                        Reset My Password
                    </a>
                </p>

                <p>
                    This password reset link will expire in 30 minutes.
                </p>

                <p>
                    If you did not request a password reset,
                    you can safely ignore this email.
                </p>
                """;

                await _goHighLevelService.SendEmailAsync(
                    contactId,
                    subject,
                    html
                );
        }
    }
}