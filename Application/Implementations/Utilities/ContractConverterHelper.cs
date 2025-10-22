using Application.Contracts;
using DomainModels;

namespace Application.Implementations.Utilities
{
    /// <summary>
    /// TODO must be replaced by the AutoMapper usage
    /// </summary>
    internal static class ContractConverterHelper
    {
        public static ChatMessage ToDatabaseEntity( this MessageDto message )
        {
            return new ChatMessage
            {
                Content = message.Content,
                CreatedTime = message.CreatedTime,
                Author = message.Author
            };
        }

    }
}
