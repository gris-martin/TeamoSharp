using System;
using System.Collections.Generic;
using System.Text;

namespace TeamoSharp.Entities
{
    public class TeamoEntry
    {
        public int? Id { get; set; }
        public ClientMessage Message { get; set; }
        public string Game { get; set; }
        public List<Member> Members { get; set; } = new List<Member>();
        public DateTime EndDate { get; set; }
        public int MaxPlayers { get; set; }
    }
}
