using System.ComponentModel.DataAnnotations;

namespace TeamoSharp.Models
{
    public class PlayMember
    {
        public int PlayMemberId { get; set; }

        [Required]
        public long DiscordUserId { get; set; }

        [Required]
        public int NumPlayers { get; set; }
    }
}
