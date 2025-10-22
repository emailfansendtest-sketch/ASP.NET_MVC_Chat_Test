using Application.Implementations.Sending;
using Application.Implementations.Streaming;
using Application.Implementations.User;
using Application.Implementations.Utilities;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI
{
    public static class Configure
    {
        public static WebApplicationBuilder AddApplicationLayer( this WebApplicationBuilder builder )
        {
            builder.Services.AddSingleton<IMessageBatchWriterService, MessageBatchWriterService>();
            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddHostedService<MessageBatchWriterWorker>();

            builder.Services.AddSingleton<IChatEventBus, ChatEventBus>();
            //builder.Services.AddSingleton<IMessageStreamWriterFactory, MessageStreamWriterFactory>();
            builder.Services.AddSingleton<IMessageStreamService, MessageStreamService>();

            builder.Services.AddSingleton<IMessageSenderService, MessageSenderService>();

            builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();
            builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
            builder.Services.AddScoped<IUserService, UserService>();

            return builder;
        }

    }
}
