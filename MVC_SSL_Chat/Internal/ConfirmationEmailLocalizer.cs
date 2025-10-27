using Contracts.Interfaces;
using Microsoft.Extensions.Localization;

namespace MVC_SSL_Chat.Internal
{
    public class ConfirmationEmailLocalizer : IConfirmationEmailLocalizer
    {
        private const string ConfirmationEmailTitleKey = "ConfirmationEmailTitle";
        private const string ConfirmationEmailBodyKey = "ConfirmationEmailBody";
        private readonly IStringLocalizer<Localization> _localizer;
        
        public ConfirmationEmailLocalizer( IStringLocalizer<Localization> localizer )
        {
            _localizer = localizer;
        }

        /// <inheritdoc />
        public string Title => _localizer[ ConfirmationEmailTitleKey ];

        /// <inheritdoc />
        public string Body => _localizer[ ConfirmationEmailBodyKey ];

    }
}
