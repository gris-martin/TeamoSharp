using System.ComponentModel.DataAnnotations;

namespace TeamoSharp.DataAccessLayer.Models
{
    public class Member
    {
        public int MemberId { get; set; }

        [Required]
        public string ClientUserId { get; set; }

        [Required]
        public int NumPlayers { get; set; }
        public int PostId { get; set; }
    }
}
