using Application.Contracts;
using Application.Interfaces.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Utilities
{
    public interface IChatEventBus
    {
        Task PublishAsync( MessageDto message );
        void Subscribe( IMessageStreamWriter listener );
        void Unsubscribe( IMessageStreamWriter listener );
    }
}
