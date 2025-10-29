using NLog.Extensions.Logging;

namespace MVC_SSL_Chat.Logging
{
    public static class LoggingExtensions
    {
        public static void AddCustomLogging( this WebApplicationBuilder builder )
        {
            builder.Logging.ClearProviders();

            builder.Logging.SetMinimumLevel( LogLevel.Trace );

            var nLogLoggingConfiguration =
                new NLogLoggingConfiguration(
                    builder.Configuration.GetSection( "NLog" )
                );
            builder.Logging.AddNLog( nLogLoggingConfiguration );
        }

    }
}
