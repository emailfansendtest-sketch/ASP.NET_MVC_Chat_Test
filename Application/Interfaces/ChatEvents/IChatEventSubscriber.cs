using DomainModels;

namespace Application.Interfaces.ChatEvents
{
    internal interface IChatEventSubscriber : IDisposable
    {
        void TryWrite( ChatMessage message );
    }
}
