using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace TeamoSharp.ErrorHandling
{
    public static class DiscordPoster
    {
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
            embedBuilder.AddField(e.GetType().ToString(), e.Message);
            embedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "See log for more information"
            };
            AddInnerException(embedBuilder, e, 1);
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

        public async static Task PostExceptionMessageAsync(Exception e, DiscordChannel channel, string s = null)
        {
            var embed = BuildEmbed(e, s);
            var message = await channel.SendMessageAsync(embed: embed);
            await Task.Delay(15000);
            await message.DeleteAsync();
        }
    }
}
