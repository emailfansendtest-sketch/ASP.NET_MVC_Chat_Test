using Application.Implementations.Sending;
using Application.Implementations.Streaming;
using Application.Implementations.User;
using Application.Implementations.Utilities;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI
{
    public static class Configure
    {
        public static IServiceCollection AddApplicationLayer( this IServiceCollection services )
        {
            services.AddSingleton<IMessageBatchWriterService, MessageBatchWriterService>();
            services.AddSingleton<IClock, SystemClock>();
            services.AddHostedService<MessageBatchWriterWorker>();

            services.AddSingleton<IChatEventBus, ChatEventBus>();
            services.AddSingleton<IMessageStreamService, MessageStreamService>();

            services.AddSingleton<IMessageSenderService, MessageSenderService>();

            services.AddScoped<IUserRepository, IdentityUserRepository>();
            services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }

    }
}
