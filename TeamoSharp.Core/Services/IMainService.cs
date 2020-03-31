using System;
using System.Threading.Tasks;
using TeamoSharp.Entities;

namespace TeamoSharp.Services
{
    public interface IMainService
    {
        Task CreateAsync(TeamoEntry entry);
        Task EditDateAsync(DateTime date, int postId);
        Task EditMaxPlayersAsync(int numPlayers, int postId);
        Task EditGameAsync(string game, int postId);
        Task DeleteAsync(int postId);
        Task AddMemberAsync(Member member, ClientMessage message);
        Task RemoveMemberAsync(string userId, ClientMessage message);
    }
}
