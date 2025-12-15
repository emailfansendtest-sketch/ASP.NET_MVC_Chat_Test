using Application.Implementations.ChatEvents;
using Application.Implementations.EntityCreation;
using Application.Implementations.Sending;
using Application.Implementations.Streaming;
using Application.Implementations.User;
using Application.Implementations.Utilities;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.EntityCreation;
using Application.Interfaces.Sending;
using Application.Interfaces.Streaming;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI
{
    public static class ConfigureExtensions
    {
        public static IServiceCollection AddApplicationLayer( this IServiceCollection services )
        {
            services.AddSingleton<IMessageWriterService, MessageBatchWriterService>();
            services.AddSingleton<IClock, SystemClock>();
            services.AddHostedService<MessageBatchWriterWorker>();

            services.AddSingleton<IChatEventBus, ChatEventBus>();
            services.AddSingleton<IChatEventSubscriberFactory, ChatEventSubscriberFactory>();
            services.AddSingleton<IMessageStreamService, MessageStreamService>();

            services.AddScoped<IMessageSenderService, MessageSenderService>();

            services.AddScoped<IUserRepository, IdentityUserRepository>();
            services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChatMessageDtoFactory, ChatMessageDtoFactory>();

            return services;
        }

    }
}
