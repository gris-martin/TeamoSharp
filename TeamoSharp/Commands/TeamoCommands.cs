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
        private readonly IMainService _playService;

        public TeamoCommands(
            ILogger<TeamoCommands> logger, IMainService playService)
        {
            _logger = logger;
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
    }
}
