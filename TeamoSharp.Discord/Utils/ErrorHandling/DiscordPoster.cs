using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TeamoSharp.ErrorHandling
{
    public static class DiscordPoster
    {
        private const int RemoveTimerMs = 15000;

        private static DiscordEmbed BuildEmbed(Exception e, string s = null)
        {
            var embedBuilder = new DiscordEmbedBuilder();

            if (s is null)
            {
                embedBuilder.Description = "Something went wrong!";
            } else
            {
                embedBuilder.Description = s;
            }

            if (!(e is null))
            {
                embedBuilder.AddField(e.GetType().ToString(), e.Message);
                AddInnerException(embedBuilder, e, 1);
            }

            embedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"See log for more information. This message will be deleted in {RemoveTimerMs / 1000.0} seconds"
            };

            return embedBuilder.Build();
        }

        private static void AddInnerException(DiscordEmbedBuilder builder, Exception e, int i)
        {
            var inner = e.InnerException;
            if (!(inner is null))
            {
                builder.AddField($"Inner exception {i}: {inner.GetType().ToString()}", inner.Message);
                AddInnerException(builder, inner, i + 1);
            }
        }

        public async static Task PostExceptionMessageAsync(DiscordChannel channel, ILogger logger, Exception e = null, string s = null)
        {
            logger.LogError(e, s);

            var embed = BuildEmbed(e, s);
            var message = await channel.SendMessageAsync(embed: embed);
            await Task.Delay(RemoveTimerMs);
            try
            {
                await message.DeleteAsync();
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                logger.LogInformation("Could not delete message since it had already been deleted");
            }
        }
    }
}
