
namespace Contracts.Interfaces
{
    /// <summary>
    /// Provides the access to the confirmation email localized data.
    /// </summary>
    public interface IConfirmationEmailLocalizer
    {
        /// <summary>
        /// Confirmation email title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Confirmation email content.
        /// </summary>
        string Body { get; }
    }
}
