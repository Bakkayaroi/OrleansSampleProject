using Server.Grains.Interfaces;

namespace Server.Grains;

public class MatchmakingGrain : Grain, IMatchmakingGrain
{
    private List<IPlayerGrain> _queue = new();

    public Task AddPlayerToQueue(IPlayerGrain player)
    {
        _queue.Add(player);

        if (_queue.Count >= 2)
        {
            var players = _queue.Take(2).ToList();
            _queue.RemoveRange(0, 2);
            var lobby = GrainFactory.GetGrain<ILobbyGrain>(Guid.NewGuid());
            lobby.AddPlayers(players);
        }

        return Task.CompletedTask;
    }
}