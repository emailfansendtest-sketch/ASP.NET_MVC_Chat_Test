using Microsoft.EntityFrameworkCore;
using SecuritySupplements.Contracts;
using System.Reflection;

namespace Storage
{
    /// <summary>
    /// The implementation of the database context factory interface.
    /// </summary>
    public class ChatDbContextFactory : IDbContextFactory<ChatDbContext>
    {
        private readonly ISensitiveDataProvider _sensitiveDataProvider;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;

        /// <inheritdoc />
        public ChatDbContextFactory(
            ISensitiveDataProvider sensitiveDataProvider,
            ISecretsReadinessTracker secretsReadinessTracker )
        {
            _sensitiveDataProvider = sensitiveDataProvider;
            _secretsReadinessTracker = secretsReadinessTracker;
        }

        public ChatDbContext CreateDbContext( )
        {
            _secretsReadinessTracker.WaitUntilReady();

            var connectionString = _sensitiveDataProvider.DatabaseConnectionString;
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseNpgsql( connectionString,
                    b => b.MigrationsAssembly( Assembly.GetExecutingAssembly().GetName().Name ) )
                .Options;

            return new ChatDbContext( options );
        }
        public Task<ChatDbContext> CreateDbContextAsync( CancellationToken cancellationToken = default )
            => Task.FromResult( CreateDbContext() );
    }
}
