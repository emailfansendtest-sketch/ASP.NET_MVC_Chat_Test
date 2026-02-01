using MVC_SSL_Chat.Middleware;
using MVC_SSL_Chat.DI;
using MVC_SSL_Chat.HealthChecks;
using MVC_SSL_Chat.Antiforgery;
using MVC_SSL_Chat.Logging;

internal class Program
{
    public static void Main( string[] args )
    {
        var builder = WebApplication.CreateBuilder( args );

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.Services.AddLocalization( options => options.ResourcesPath = "Resources" );
        builder.AddCustomAntiforgery();


        builder.AddCustomLogging();

        builder.AddCustomDependencies();
        builder.AddCustomHealthChecks();

        var app = builder.Build();

        app.UseRequestLocalization(  );

        // Configure the HTTP request pipeline.
        if(!app.Environment.IsDevelopment())
        {

            app.UseExceptionHandler( "/Home/Error" );
            app.UseHsts();
        }

        // 
        var httpsRedirectEnabled = builder.Configuration.GetValue<bool>( "HttpsRedirection:Enabled" );

        if( httpsRedirectEnabled )
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCustomMiddleware();

        app.MapCustomAntiforgery();
        app.MapCustomHealthChecks();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}" );
        app.MapRazorPages();

        app.Run();
    }
}
