using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Storage
{
    /// <summary>
    /// The implementation of the database context factory interface.
    /// </summary>
    public class ChatDbContextFactory : IDbContextFactory<ChatDbContext>
    {
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;
        private readonly IOptionsMonitor<DatabaseOptions> _databaseMonitor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="databaseMonitor">The database connection options monitor.</param>
        /// <param name="secretsReadinessTracker">The source of the signal to confirm the database connection is obtained.</param>
        public ChatDbContextFactory(
            IOptionsMonitor<DatabaseOptions> databaseMonitor,
            ISecretsReadinessTracker secretsReadinessTracker )
        {
            _secretsReadinessTracker = secretsReadinessTracker;
            _databaseMonitor = databaseMonitor;
        }

        /// <inheritdoc />
        public ChatDbContext CreateDbContext( )
        {
            _secretsReadinessTracker.WaitUntilReady();

            var databaseOptions = _databaseMonitor.Get( nameof( DatabaseOptions ) );
            var connectionString = databaseOptions.ConnectionString;
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
