using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace TeamoSharp.Utils
{
    public static class DiscordEmojiUtils
    {
        public static readonly string[] NumberEmojiUnicodes =
        {
            "1️⃣",
            "2️⃣",
            "3️⃣",
            "4️⃣",
            "5️⃣",
            "6️⃣",
            "7️⃣",
            "8️⃣",
            "9️⃣",
            "🔟"
        };

        public static readonly string[] NumberEmojiNames =
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

        public static readonly Dictionary<string, string> EmojiNameToUnicode = new Dictionary<string, string>()
        {
            {":one:", "1️⃣" },
            {":two:", "2️⃣" },
            {":three:", "3️⃣" },
            {":four:", "4️⃣" },
            {":five:", "5️⃣" },
            {":six:", "6️⃣" },
            {":seven:", "7️⃣" },
            {":eight:", "8️⃣" },
            {":nine:", "9️⃣" },
            {":keycap_ten:", "🔟" }
        };

        public static DiscordEmoji CreateEmojiFromNumber(int number)
        {
            if (number < 1 || number > 10)
            {
                // TODO: Better exception
                throw new Exception($"Cannot convert the number {number} to an emoji! Only numbers between 1 and 10 are supported.");
            }

            var emoji = DiscordEmoji.FromUnicode(NumberEmojiUnicodes[number - 1]);
            return emoji;
        }

        public static DiscordEmoji GetCancelEmoji()
        {
            return DiscordEmoji.FromUnicode("❌");
        }
    }
}
