using System.Threading;
using System.Threading.Tasks;

namespace Service.Background
{
    public interface IEmailBackgroundQueue
    {
        ValueTask QueueEmailAsync(EmailMessage message);
        ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
