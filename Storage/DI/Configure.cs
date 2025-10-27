using Contracts.Interfaces;
using DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Storage.DI
{
    public static class Configure
    {
        public static IServiceCollection AddStorageLayer( this IServiceCollection services )
        {
            services.AddDbContextFactory<ChatDbContext, ChatDbContextFactory>();
            services.AddScoped( p => p.GetRequiredService<IDbContextFactory<ChatDbContext>>().CreateDbContext() );

            services.AddDefaultIdentity<ChatUser>(
                options => options.SignIn.RequireConfirmedAccount = true )
                .AddEntityFrameworkStores<ChatDbContext>();

            services.AddSingleton<IDbService, DbService>();

            return services;
        }
    }
}
