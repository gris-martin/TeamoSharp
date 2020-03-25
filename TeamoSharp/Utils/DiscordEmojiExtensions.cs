using DSharpPlus.Entities;
using System;
using System.Linq;

namespace TeamoSharp.Extensions
{
    public static class DiscordEmojiExtensions
    {
        private static readonly string[] NumberEmojiNames =
        {
            ":one:",
            ":two:",
            ":three:",
            ":four:",
            ":five:",
            ":six:",
            ":seven:",
            ":eight:",
            ":nine:",
            ":keycap_ten:"
        };

        public static int? GetAsNumber(this DiscordEmoji emoji)
        {
            var emojiIndex = Array.IndexOf(NumberEmojiNames, emoji.GetDiscordName());
            return (emojiIndex < 0 || emojiIndex > 9) ? (int?)null : emojiIndex;
        }

        public static bool IsCancelEmoji(this DiscordEmoji emoji)
        {
            return emoji.GetDiscordName() == ":x:";
        }

        public static bool IsNumberEmoji(this DiscordEmoji emoji)
        {
            return NumberEmojiNames.Contains(emoji.GetDiscordName());
        }
    }
}
