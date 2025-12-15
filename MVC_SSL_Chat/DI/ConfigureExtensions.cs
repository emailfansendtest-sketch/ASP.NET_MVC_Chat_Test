using Application.DI;
using Application.Interfaces.Streaming;
using Contracts.Options;
using Contracts.Interfaces;
using Email.DI;
using MVC_SSL_Chat.Internal;
using SecuritySupplements.DI;
using Storage.DI;

namespace MVC_SSL_Chat.DI
{
    public static class ConfigureExtensions
    {
        private static WebApplicationBuilder AddSolutionLayers( this WebApplicationBuilder builder )
        {
            builder.Services.AddStorageLayer();
            builder.Services.AddSecurityLayer();
            builder.Services.AddEmailLayer();
            builder.Services.AddSingleton<IMessageStreamWriterFactory, MessageStreamWriterFactory>();
            builder.Services.AddSingleton<IConfirmationEmailLocalizer, ConfirmationEmailLocalizer>();
            builder.Services.AddApplicationLayer();
            return builder;
        }

        private static WebApplicationBuilder AddSolutionOptions( this WebApplicationBuilder builder )
        {
            builder.Services.Configure<DatabaseClientOptions>( builder.Configuration.GetSection( DatabaseClientOptions.ConfigKey ) );
            builder.Services.Configure<SensitiveDataClientOptions>( builder.Configuration.GetSection( SensitiveDataClientOptions.ConfigKey ) );
            builder.Services.Configure<MessageStreamOptions>( builder.Configuration.GetSection( MessageStreamOptions.ConfigKey ) );
            builder.Services.Configure<MessageWriterOptions>( builder.Configuration.GetSection( MessageWriterOptions.ConfigKey ) );
            builder.Services.Configure<PersistenceOptions>( builder.Configuration.GetSection( PersistenceOptions.ConfigKey ) );
            builder.Services.Configure<ChatEventOptions>( builder.Configuration.GetSection( ChatEventOptions.ConfigKey ) );
            return builder;
        }

        public static WebApplicationBuilder AddCustomDependencies( this WebApplicationBuilder builder )
        {
            builder.AddSolutionLayers();
            builder.AddSolutionOptions();
            return builder;
        }
    }
}
