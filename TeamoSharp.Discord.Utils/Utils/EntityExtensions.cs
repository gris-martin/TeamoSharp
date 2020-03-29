using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using TeamoSharp.Entities;

namespace TeamoSharp.Discord.Utils.Utils
{
    public static class EntityExtensions
    {
        public static ClientMessage AsEntityType(this DiscordMessage dMessage)
        {
            return new ClientMessage
            {
                MessageId = dMessage.Id.ToString(),
                ChannelId = dMessage.ChannelId.ToString(),
                ServerId = dMessage.Channel.GuildId.ToString()
            };
        }
    }
}
