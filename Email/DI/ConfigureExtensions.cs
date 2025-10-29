using Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Email.DI
{
    /// <summary>
    /// Extension for '<see cref="IServiceCollection"/>' (Microsoft.Extensions.DependencyInjection)
    /// </summary>
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Add email implementations to DI.
        /// </summary>
        /// <param name="serviceCollection">Services.</param>
        /// <returns></returns>
        public static IServiceCollection AddEmailLayer( this IServiceCollection serviceCollection )
        {
            serviceCollection.AddSingleton<IConfirmationEmailSender, FreeConfirmationEmailMessageSender>();
            return serviceCollection;
        }
    }
}
