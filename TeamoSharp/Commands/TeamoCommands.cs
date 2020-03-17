using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamoSharp.Services;

namespace TeamoSharp.Commands
{
    public class TeamoCommands : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly ITeamoService _teamoService;
        private readonly IPlayService _playService;

        public TeamoCommands(
            ILogger<TeamoCommands> logger, ITeamoService teamoService, IPlayService playService)
        {
            _logger = logger;
            _teamoService = teamoService;
            _playService = playService;
        }

        [Command("create")]
        [Description("Create a new teamo")]
        public async Task Create(CommandContext ctx)
        {
            DateTime endDate = DateTime.Now + new TimeSpan(0, 0, 20);
            string game = "League of LoL";
            int maxPlayers = 5;
            var channel = ctx.Channel;
            try
            {
                await _playService.CreateAsync(endDate, maxPlayers, game, channel.Id, ctx.Client);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not create a new teamo");
                await ErrorHandling.DiscordPoster.PostExceptionMessageAsync(e, channel, "Could not create a new teamo :(");
                return;
            }
        }


        [Command("delete")]
        [Aliases("stop")]
        [Description("Create a new teamo")]
        public async Task Delete(CommandContext ctx, [RemainingText] string postIdString)
        {
            if (!int.TryParse(postIdString, out int postId))
            {
                _logger.LogInformation($"Delete command argument \"{postIdString}\"could not be converted to int");
                return;
            }

            try
            {
                await _playService.DeleteAsync(postId, ctx.Client);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not delete teamo");
                await ErrorHandling.DiscordPoster.PostExceptionMessageAsync(e, ctx.Channel, "Could not delete teamo");
                return;
            }
        }


        [Command("add")]
        [Description("Add a random object to the database")]
        public async Task Add(CommandContext ctx)
        {
            _logger.LogInformation($"{ctx.User.Username} created a new post in guild {ctx.Guild.Name}! Channel: {ctx.Channel.Name}!");
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));

            var post = new Models.PlayPost
            {
                DiscordChannelId = (long)ctx.Channel.Id,
                DiscordMessageId = (long)ctx.Message.Id,
                EndDate = new DateTime(2020, 03, 12, 22, 43, 25),
                Game = "League of Legends",
                MaxPlayers = 5
            };
            post.Members.Add(new Models.PlayMember());

            await _teamoService.CreateNewPostAsync(post).ConfigureAwait(false);
            //await _playDiscordService.CreateMessageAsync(post, ctx.Channel);
        }

        [Command("print")]
        [Description("Print the first entry of the database")]
        public async Task Print(CommandContext ctx)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));

            ulong id = await _teamoService.GetMessageIdAsync().ConfigureAwait(false);
            int numMessages = _teamoService.NumberOfMessages();
            await ctx.Channel.SendMessageAsync($"{id}: {numMessages}").ConfigureAwait(false);
        }

    }
}
