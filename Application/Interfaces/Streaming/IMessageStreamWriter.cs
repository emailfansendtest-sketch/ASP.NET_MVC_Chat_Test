using Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Streaming
{
    public interface IMessageStreamWriter
    {
        Task WriteMessageAsync( MessageDto message, CancellationToken ct = default );

        Task WriteKeepAliveAsync( CancellationToken ct = default );
    }
}
