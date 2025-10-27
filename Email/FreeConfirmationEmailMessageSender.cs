using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using Microsoft.Extensions.Options;
using Contracts.Options;

namespace Email
{
    /// <summary>
    /// The confirmation email sender implementation tailored to use the free means of delivering the email message.
    /// </summary>
    internal class FreeConfirmationEmailMessageSender : IConfirmationEmailSender
    {
        private readonly ILogger<FreeConfirmationEmailMessageSender> _logger;
        private readonly IConfirmationEmailLocalizer _localizer;
        private readonly IOptionsMonitor<SmtpOptions> _smtpMonitor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="smtpMonitor">The smtp server settings.</param>
        /// <param name="localizer">The localizer to provide the email title and content.</param>
        public FreeConfirmationEmailMessageSender(
            ILogger<FreeConfirmationEmailMessageSender> logger,
            IOptionsMonitor<SmtpOptions> smtpMonitor,
            IConfirmationEmailLocalizer localizer )
        {
            _logger = logger;
            _localizer = localizer;
            _smtpMonitor = smtpMonitor;
        }

        public async Task SendConfirmationAsync( string address, string callbackUrl )
        {
            _logger.Log( LogLevel.Trace, "Creating the network connection." );

            var options = _smtpMonitor.Get( nameof( SmtpOptions ) );

            var credential = new NetworkCredential( userName: options.FromAddress,
                    password: options.FromPassword );
            var client = new SmtpClient( host: options.Host,
                port: options.Port )
            {
                Credentials = credential,
                EnableSsl = true
            };

            var emailBody = string.Format( _localizer.Body, callbackUrl );

            try
            {
                _logger.Log( LogLevel.Trace, "Attempting to send the confirmation email." );

                await client.SendMailAsync( from: options.FromAddress,
                    recipients: address,
                    subject: _localizer.Title,
                    body: emailBody );

                _logger.Log( LogLevel.Trace, "Confirmation email is sent successfully." );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Confirmation email is not sent." );
            }

        }

    }
}
