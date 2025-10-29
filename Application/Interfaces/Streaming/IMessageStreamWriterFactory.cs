using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Streaming
{
    /// <summary>
    /// Creates <see cref="IMessageStreamWriter"/> instances bound to a specific HTTP response.
    /// Used by the web layer to obtain a per-connection writer for streaming.
    /// </summary>
    public interface IMessageStreamWriterFactory
    {
        /// <summary>
        /// Creates a writer that will send data to the given <paramref name="response"/>.
        /// The caller owns the response lifetime; the writer should not outlive it.
        /// </summary>
        /// <param name="response">HTTP response associated with the current streaming connection.</param>
        /// <returns>A transport writer for the current client connection.</returns>
        IMessageStreamWriter Create( HttpResponse response );
    }
}
