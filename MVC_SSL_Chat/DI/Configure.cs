using Application.Interfaces.Streaming;
using MVC_SSL_Chat.HealthCheck;
using MVC_SSL_Chat.Internal;
using SecuritySupplements.DI;
using Storage.DI;
using Email;
using Application.DI;
using Contracts.Options;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Contracts.Interfaces;

namespace MVC_SSL_Chat.DI
{
    public static class Configure
    {
        private static WebApplicationBuilder AddSolutionLayers( this WebApplicationBuilder builder )
        {
            builder.Services.AddStorageLayer();
            builder.Services.AddSecurityLayer();
            builder.Services.AddEmailImplementations();
            builder.Services.AddSingleton<IMessageStreamWriterFactory, MessageStreamWriterFactory>();
            builder.Services.AddSingleton<IConfirmationEmailLocalizer, ConfirmationEmailLocalizer>();
            builder.Services.AddApplicationLayer();
            builder.Services.AddHealthChecks()
                .AddCheck<VaultHealthCheck>( "vault_readiness_check" );
            return builder;
        }

        private static WebApplicationBuilder AddSolutionOptions( this WebApplicationBuilder builder )
        {
            builder.Services.Configure<SensitiveDataClientOptions>( builder.Configuration.GetSection( SensitiveDataClientOptions.ConfigKey ) );
            builder.Services.Configure<MessageStreamOptions>( builder.Configuration.GetSection( MessageStreamOptions.ConfigKey ) );
            builder.Services.Configure<PersistenceOptions>( builder.Configuration.GetSection( PersistenceOptions.ConfigKey ) );
            return builder;
        }

        private static WebApplicationBuilder AddLocalization( this WebApplicationBuilder builder )
        {
            builder.Services.AddLocalization( options => options.ResourcesPath = "Resources" );

            return builder;
        }

        public static WebApplicationBuilder AddCustomParts( this WebApplicationBuilder builder )
        {
            builder.AddSolutionLayers();
            builder.AddSolutionOptions();
            builder.AddLocalization();
            return builder;
        }
    }
}
