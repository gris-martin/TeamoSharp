using DSharpPlus.Entities;
using System;
using static TeamoSharp.Utils.DiscordEmojiUtils;

namespace TeamoSharp.Utils
{
    public static class DiscordEmojiExtensions
    {
        public static int GetAsNumber(this DiscordEmoji emoji)
        {
            var emojiName = emoji.GetDiscordName();
            var emojiUnicode = EmojiNameToUnicode[emojiName];
            var emojiIndex = Array.IndexOf(NumberEmojiUnicodes, emojiUnicode);
            return emojiIndex + 1;
        }

        public static bool IsCancelEmoji(this DiscordEmoji emoji)
        {
            return emoji.GetDiscordName() == ":x:";
        }

        public static bool IsNumberEmoji(this DiscordEmoji emoji)
        {
            return EmojiNameToUnicode.ContainsKey(emoji.GetDiscordName());
        }
    }
}
