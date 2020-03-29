using System;
using System.Threading.Tasks;
using TeamoSharp.Entities;

namespace TeamoSharp.Services
{
    public interface IClientService
    {
        Task<ClientMessage> CreateMessageAsync(TeamoEntry entry);
        Task UpdateMessageAsync(TeamoEntry entry);
        Task DeleteMessageAsync(ClientMessage message);
        Task CreateStartMessageAsync(TeamoEntry entry);
    }
}
