
namespace Contracts.Options
{
    public class MessageWriterOptions
    {
        public const string ConfigKey = "MessageWriter";

        public int MessageQueueCapacity { get; set; }

        public int MessageBatchSize { get; set; }
    }
}
