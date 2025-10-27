namespace Contracts.Interfaces
{
    /// <summary>
    /// The tracker of the sensitive data readiness.
    /// </summary>
    public interface ISecretsReadinessTracker
    {
        void WaitUntilReady( CancellationToken cancellationToken = default );
        Task WaitUntilReadyAsync( CancellationToken cancellationToken = default );
        void SignalReady();
        bool IsReady { get; }
    }
}
