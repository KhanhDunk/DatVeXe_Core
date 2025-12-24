using System.Threading.Channels;

namespace Service.Background
{
    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<EmailMessage> _channel;

        public EmailBackgroundQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<EmailMessage>(options);
        }

        public ValueTask QueueEmailAsync(EmailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return _channel.Writer.WriteAsync(message);
        }

        public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
