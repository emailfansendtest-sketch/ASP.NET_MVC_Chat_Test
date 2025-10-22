using Application.Interfaces.Utilities;

namespace Application.Implementations.Utilities
{
    /// <summary>
    /// The implementation of the current time provider.
    /// </summary>
    public class SystemClock : IClock
    {
        /// <inheritdoc />
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}
