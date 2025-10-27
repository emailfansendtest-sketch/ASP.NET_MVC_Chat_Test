using NLog.Extensions.Logging;
using MVC_SSL_Chat.Middleware;
using MVC_SSL_Chat.DI;

internal class Program
{
    public static void Main( string[] args )
    {
        var builder = WebApplication.CreateBuilder( args );

        builder.Services.AddRazorPages()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

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

        builder.Services.AddControllersWithViews()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();
        builder.AddCustomParts();

        var app = builder.Build();

        app.UseRequestLocalization(  );

        // Configure the HTTP request pipeline.
        if(!app.Environment.IsDevelopment())
        {

            app.UseExceptionHandler( "/Home/Error" );
            app.UseHsts();
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