namespace TaskManager.Applab.Application.Services
{
    public class UnconfiguredEmailService : Interfaces.IEmailService
    {
        public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            throw new NotImplementedException(
                "No real email provider is configured. Wire up SMTP/SendGrid/Azure Communication " +
                "Services here, then swap this class for the real one in Program.cs.");
        }
    }
}