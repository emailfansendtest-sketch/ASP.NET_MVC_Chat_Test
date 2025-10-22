
using DomainModels;

namespace Application.Interfaces.Sending
{
    public interface IMessageSenderService
    {
        Task SendAsync( string content, ChatUser author, CancellationToken ct = default );
    }
}
