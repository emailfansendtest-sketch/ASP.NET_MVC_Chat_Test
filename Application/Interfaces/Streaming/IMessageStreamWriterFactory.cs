using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Streaming
{
    public interface IMessageStreamWriterFactory
    {
        IMessageStreamWriter Create( HttpResponse response );
    }
}
