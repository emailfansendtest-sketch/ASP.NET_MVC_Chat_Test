using Serilog;

namespace MVC_SSL_Chat.Logging
{
    public static class LoggingExtensions
    {
        public static void AddCustomLogging( this WebApplicationBuilder builder )
        {
            // Don't let the default providers (Console, Debug, EventSource, etc.) interfere.
            builder.Logging.ClearProviders();

            // Avoid ASP.NET Core's min-level filter blocking events that Serilog would otherwise handle.
            // (Serilog's own MinimumLevel is controlled via appsettings.)
            builder.Logging.SetMinimumLevel( LogLevel.Trace );

            // Safety: prevent Windows-only sink config from being used on non-Windows (e.g., Docker/Linux).
            if(!OperatingSystem.IsWindows())
            {
                var writeTo = builder.Configuration.GetSection( "Serilog:WriteTo" ).GetChildren();
                var hasEventLogSink = writeTo.Any( s =>
                    string.Equals( s[ "Name" ], "EventLog", StringComparison.OrdinalIgnoreCase ) );

                if(hasEventLogSink)
                {
                    throw new InvalidOperationException(
                        "Serilog EventLog sink is configured, but the current OS is not Windows. " +
                        "Move the EventLog sink to a Windows-only environment config (e.g., appsettings.Production.json on Windows) " +
                        "and use appsettings.Docker.Production.json in containers."
                    );
                }
            }

            // Build Serilog logger from configuration (Serilog section in appsettings*.json).
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration( builder.Configuration )
                .Enrich.FromLogContext()
                .CreateLogger();

            // Plug Serilog into Microsoft.Extensions.Logging
            builder.Logging.AddSerilog( logger: Log.Logger, dispose: true );
        }
    }
}