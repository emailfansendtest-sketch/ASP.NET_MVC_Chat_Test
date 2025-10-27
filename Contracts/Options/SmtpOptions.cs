
namespace Contracts.Options
{
    /// <summary>
    /// SMTP remote server usage settings.
    /// </summary>
    public sealed class SmtpOptions
    {
        /// <summary>
        /// SMTP host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTP port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The address of the email account to access via the SMTP.
        /// </summary>
        public string FromAddress { get; set; } = string.Empty;

        /// <summary>
        /// 'From' field value of the email to send via the SMTP.
        /// </summary>
        public string? FromDisplayName { get; set; }

        /// <summary>
        /// The password of the email account to access via the SMTP.
        /// </summary>
        public string FromPassword { get; set; } = string.Empty;
    }
}
