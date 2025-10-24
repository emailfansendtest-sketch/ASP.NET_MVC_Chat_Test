
namespace SecuritySupplements.Contracts
{
    public interface ISecretsReadinessTracker
    {
        void WaitUntilReady( CancellationToken cancellationToken = default );
        Task WaitUntilReadyAsync( CancellationToken cancellationToken = default );
        void SignalReady();
        bool IsReady { get; }
    }
}
