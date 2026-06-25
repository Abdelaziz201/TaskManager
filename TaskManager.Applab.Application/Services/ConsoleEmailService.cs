using Microsoft.Extensions.Logging;
using TaskManager.Applab.Application.Interfaces;

namespace TaskManager.Applab.Application.Services
{
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            _logger.LogInformation(
                "=== PASSWORD RESET (DEV MODE — no real email sent) ===\nTo: {Email}\nLink: {Link}\n=======================================================",
                toEmail, resetLink);

            return Task.CompletedTask;
        }
    }
}