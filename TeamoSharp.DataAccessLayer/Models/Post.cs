using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamoSharp.DataAccessLayer.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required]
        public ClientMessage Message { get; set; }

        [Required]
        public string Game { get; set; }
        [Required]
        public List<Member> Members { get; } = new List<Member>();
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int MaxPlayers { get; set; }
    }
}
