using SecuritySupplements.Contracts;

namespace SecuritySupplements
{
    internal class SecretsReadinessTracker : ISecretsReadinessTracker
    {
        private readonly TaskCompletionSource _tcs = new( TaskCreationOptions.RunContinuationsAsynchronously );

        public Task WaitUntilReadyAsync( CancellationToken cancellationToken = default )
            => _tcs.Task.WaitAsync( cancellationToken );
        public void WaitUntilReady( CancellationToken cancellationToken = default )
            => _tcs.Task.Wait(cancellationToken );

        public void SignalReady()
        {
            _tcs.TrySetResult();
        }

        public bool IsReady => _tcs.Task.IsCompletedSuccessfully;
    }
}
