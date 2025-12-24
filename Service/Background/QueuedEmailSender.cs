using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Background
{
    public class QueuedEmailSender : BackgroundService
    {
        private readonly IEmailBackgroundQueue _queue;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<QueuedEmailSender> _logger;

        public QueuedEmailSender(IEmailBackgroundQueue queue, IEmailSender emailSender, ILogger<QueuedEmailSender> logger)
        {
            _queue = queue;
            _emailSender = emailSender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email background sender started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var message = await _queue.DequeueAsync(stoppingToken);
                    try
                    {
                        await _emailSender.SendEmailAsync(message.To, message.Subject, message.Body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to {Email}", message.To);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Email background sender stopping");
            }
        }
    }
}
