using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TeamoSharp.Entities;
using TeamoSharp.Services;

namespace TeamoSharp.Tests
{
    public class TestClientService : IClientService
    {
        private readonly ILogger _logger;
        private int _id;

        public TestClientService(ILogger<TestClientService> logger)
        {
            _logger = logger;
        }

        public async Task<ClientMessage> CreateMessageAsync(TeamoEntry entry)
        {
            _logger.LogInformation($"Creating client message for entry {entry.Id}");

            var messageWithId = new ClientMessage
            {
                ServerId = entry.Message.ServerId,
                ChannelId = entry.Message.ChannelId,
                MessageId = _id.ToString()
            };
            _id++;

            await Task.Delay(1000);

            return messageWithId;
        }

        public async Task CreateStartMessageAsync(TeamoEntry entry)
        {
            _logger.LogInformation($"Creating start message for entry {entry.Id}");
            await Task.Delay(1000);
        }

        public async Task DeleteMessageAsync(ClientMessage message)
        {
            _logger.LogInformation($"Deleting message {message.MessageId}");
            await Task.Delay(1000);
        }

        public async Task UpdateMessageAsync(TeamoEntry entry)
        {
            _logger.LogInformation($"Updating message for entry {entry.Id}");
            await Task.Delay(1000);
        }
    }
}
