using Microsoft.AspNetCore.Mvc;

using MVC_SSL_Chat.Models;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;
using Microsoft.AspNetCore.Authorization;
using Application.Exceptions;

namespace MVC_SSL_Chat.Controllers
{
    [Route( "chat" )]
    public class ChatController : Controller
    {
        private readonly IMessageStreamWriterFactory _writerFactory;
        private readonly IMessageStreamService _streamService;
        private readonly IMessageSenderService _senderService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IMessageStreamWriterFactory writerFactory,
            IMessageStreamService streamService,
            IMessageSenderService senderService,
            ILogger<ChatController> logger )
        {
            _writerFactory = writerFactory;
            _streamService = streamService;
            _senderService = senderService;
            _logger = logger;
        }

        [HttpGet( "stream" )]
        [Authorize]
        public async Task Stream( CancellationToken ct )
        {
            _logger.LogTrace( "Streaming the messages." );
            try
            {
                Response.Headers[ "Content-Type" ] = "text/event-stream";
                Response.Headers[ "Cache-Control" ] = "no-cache";
                Response.Headers[ "Connection" ] = "keep-alive";

                var writer = _writerFactory.Create( Response );

                await _streamService.StreamForClientAsync( writer, ct );
            }
            catch(Exception ex)
            {
                _logger.LogError( ex, "Error while performing the message streaming." );
                throw;
            }
        }

        [HttpPost( "send" )]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAsync( [FromBody] string content, CancellationToken ct )
        {
            if(!ModelState.IsValid)
                return BadRequest( ModelState );

            _logger.LogTrace( "Sending the message." );

            try
            {
                await _senderService.SendAsync( content, ct );
                return Ok();
            }
            catch( UserNotFoundException )
            {
                _logger.LogError( "Attempt of message sending by the unauthorized user." );
                return Unauthorized();
            }

            catch ( Exception ex )
            {
                _logger.LogError( ex, "Error while sending the message." );
                throw;
            }
        }
    }
}
