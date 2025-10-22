using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using MVC_SSL_Chat.Models;
using Storage.Contracts;
using MVC_SSL_Chat.Internal;
using DomainModels;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;
using Microsoft.Extensions.Options;
using Application.Interfaces.User;

namespace MVC_SSL_Chat.Controllers
{
    [Route( "chat" )]
    public class ChatController : Controller
    {
        private readonly IMessageStreamWriterFactory _writerFactory;
        private readonly IMessageStreamService _streamService;
        private readonly IMessageSenderService _senderService;
        private readonly IUserService _userService; // lightweight accessor returning username/id

        public ChatController(
            IMessageStreamWriterFactory writerFactory,
            IMessageStreamService streamService,
            IMessageSenderService senderService,
            IUserService userService )
        {
            _writerFactory = writerFactory;
            _streamService = streamService;
            _senderService = senderService;
            _userService = userService;
        }

        [HttpGet( "stream" )]
        public async Task Stream( CancellationToken ct )
        {
            Response.Headers[ "Content-Type" ] = "text/event-stream";
            Response.Headers[ "Cache-Control" ] = "no-cache";
            Response.Headers[ "Connection" ] = "keep-alive";

            var writer = _writerFactory.Create( Response );

            // leave orchestration to application service
            await _streamService.StreamForClientAsync( writer, ct );
        }

        [HttpPost( "send" )]
        public async Task<IActionResult> SendAsync( [FromBody] MessageViewModel contract, CancellationToken ct )
        {
            // minimal: validate contract, get user name from claims accessor (no DB call)
            if(!ModelState.IsValid)
                return BadRequest( ModelState );
            var user = await _userService.GetCurrentUserAsync( );
            await _senderService.SendAsync( contract.Content!, user!, ct );
            return Ok();
        }
    }
}
