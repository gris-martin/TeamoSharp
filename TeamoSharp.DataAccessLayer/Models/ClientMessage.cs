using System.ComponentModel.DataAnnotations;

namespace TeamoSharp.DataAccessLayer.Models
{
    public class ClientMessage
    {
        public int ClientMessageId { get; set; }

        [Required]
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string ServerId { get; set; }
        public int PostId { get; set; }
    }
}
