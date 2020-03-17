using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.Models;

namespace TeamoSharp.Services
{
    public interface ITeamoService
    {
        Task CreateNewPostAsync(PlayPost post);
        Task<ulong> GetMessageIdAsync();
        int NumberOfMessages();
    }

    public class TeamoService : ITeamoService
    {
        readonly ITimerService _timerService;

        public TeamoService(ITimerService timerService)
        {
            _timerService = timerService;
        }

        public async Task CreateNewPostAsync(PlayPost post)
        {
            if (post is null)
                throw new ArgumentNullException(nameof(post));

            using var context = new PlayContext();

            context.Add(post);
            _timerService.AddTimer((ulong)post.DiscordMessageId, DateTime.Now + new TimeSpan(0, 0, 5), () => Console.WriteLine($"Post fininshed: {(ulong)post.DiscordMessageId}"));

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<ulong> GetMessageIdAsync()
        {
            using var context = new PlayContext();
            var post = await context.Posts.FirstAsync().ConfigureAwait(false);
            //var post = context.Posts.ElementAt(0);
            return (ulong)post.DiscordMessageId;
        }

        public int NumberOfMessages()
        {
            using var contxt = new PlayContext();
            return contxt.Posts.ToList().Count;
        }
    }
}
