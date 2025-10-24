using MVC_SSL_Chat.Internal;
using SecuritySupplements.DI;
using Storage.DI;
using Email;
using NLog.Extensions.Logging;
using Application.Interfaces.Streaming;
using Application.DI;
using MVC_SSL_Chat.Middleware;
using SecuritySupplements;
using SecuritySupplements.Contracts;
using Storage.Contracts;
using MVC_SSL_Chat.HealthCheck;
internal class Program
{
    public static void Main( string[] args )
    {
        var builder = WebApplication.CreateBuilder( args );

        builder.Services.AddRazorPages();

        builder.Services.AddLogging( loggingBuilder =>
                     {
                         loggingBuilder.ClearProviders();
                         loggingBuilder.SetMinimumLevel( LogLevel.Trace );

                         var nLogLoggingConfiguration =
                             new NLogLoggingConfiguration(
                                 builder.Configuration.GetSection( "NLog" )
                             );

                         loggingBuilder.AddNLog( nLogLoggingConfiguration );
                     } );

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddStorageLayer();
        builder.Services.AddSecurityLayer();
        builder.Services.AddEmailImplementations();
        builder.Services.AddSingleton<IMessageStreamWriterFactory, MessageStreamWriterFactory>();
        builder.Services.AddApplicationLayer();
        builder.Services.AddHealthChecks()
            .AddCheck<VaultHealthCheck>( "vault_readiness_check" );

        // TODO Crutch! Remove it!
#pragma warning disable ASP0000 // Suppress the warning - getting the logging service before the build is necessary to log the information from the Hashicorp Vault communication
        var loggerFactory = builder.Services.BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>();
#pragma warning restore ASP0000

        var rootLogger = loggerFactory.CreateLogger<Program>();
        var nonSensitiveDataProvider = new NonSensitiveSettingsProvider( loggerFactory! );
        nonSensitiveDataProvider.ReadAppConfig( builder.Configuration );
        builder.Services.AddSingleton<IStorageSettingsProvider>( nonSensitiveDataProvider );
        builder.Services.AddSingleton<IReaderSettingsProvider>( nonSensitiveDataProvider );
        // TODO Crutch! Remove it!


        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        if(!app.Environment.IsDevelopment())
        {

            app.UseExceptionHandler( "/Home/Error" );
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSecretsReadinessGate();
        app.MapHealthChecks( "/health" );
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}" );

        app.MapRazorPages();

        app.Run();
    }
}