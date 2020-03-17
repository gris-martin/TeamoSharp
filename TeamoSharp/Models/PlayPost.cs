using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamoSharp.Models
{
    public class PlayPost
    {
        public int PlayPostId { get; set; }

        [Required]
        public long DiscordMessageId { get; set; }
        [Required]
        public long DiscordChannelId { get; set; }

        [Required]
        public string Game { get; set; }
        [Required]
        public List<PlayMember> Members { get; } = new List<PlayMember>();
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int MaxPlayers { get; set; }
    }
}
