using Microsoft.AspNetCore.Mvc;

using MVC_SSL_Chat.Models;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;

namespace MVC_SSL_Chat.Controllers
{
    [Route( "chat" )]
    public class ChatController : Controller
    {
        private readonly IMessageStreamWriterFactory _writerFactory;
        private readonly IMessageStreamService _streamService;
        private readonly IMessageSenderService _senderService;

        public ChatController(
            IMessageStreamWriterFactory writerFactory,
            IMessageStreamService streamService,
            IMessageSenderService senderService )
        {
            _writerFactory = writerFactory;
            _streamService = streamService;
            _senderService = senderService;
        }

        [HttpGet( "stream" )]
        public async Task Stream( CancellationToken ct )
        {
            Response.Headers[ "Content-Type" ] = "text/event-stream";
            Response.Headers[ "Cache-Control" ] = "no-cache";
            Response.Headers[ "Connection" ] = "keep-alive";

            var writer = _writerFactory.Create( Response );

            await _streamService.StreamForClientAsync( writer, ct );
        }

        [HttpPost( "send" )]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAsync( [FromBody] MessageViewModel contract, CancellationToken ct )
        {
            if(!ModelState.IsValid)
                return BadRequest( ModelState );

            await _senderService.SendAsync( contract.Content!, ct );
            return Ok();
        }
    }
}
