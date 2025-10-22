using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Streaming
{
    public interface IMessageStreamService
    {
        Task StreamForClientAsync( IMessageStreamWriter writer, CancellationToken ct );
    }
}
