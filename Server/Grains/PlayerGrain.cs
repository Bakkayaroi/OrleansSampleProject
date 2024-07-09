using Orleans.Providers;
using Orleans.Utilities;
using Server.Grains.Interfaces;
using Server.Grains.States;
using Server.Models;

namespace Server.Grains;

[StorageProvider(ProviderName = "Default")]
public class PlayerGrain : Grain<PlayerState>, IPlayerGrain
{
    private readonly ObserverManager<IPlayerClient> _clientObserver;
    private ILobbyGrain? _lobbyGrain;

    public PlayerGrain(ILogger<PlayerGrain> logger)
    {
        _clientObserver = new ObserverManager<IPlayerClient>(TimeSpan.FromMinutes(5), logger);
    }

    public Task Subscribe(IPlayerClient observer)
    {
        _clientObserver.Subscribe(observer, observer);

        return Task.CompletedTask;
    }

    public Task UnSubscribe(IPlayerClient observer)
    {
        _clientObserver.Unsubscribe(observer);

        return Task.CompletedTask;
    }

    public Task NotifyLobbyOpened(ILobbyGrain lobbyGrain)
    {
        _lobbyGrain = lobbyGrain;
        _clientObserver.Notify(client => client.LobbyOpened(lobbyGrain));
        return Task.CompletedTask;
    }

    public Task NotifyLobbyClosed(RoundResultModel roundResultModel)
    {
        _clientObserver.Notify(client => client.LobbyClosed(roundResultModel));
        _lobbyGrain = null;
        return Task.CompletedTask;
    }

    public Task SendNumber(int number)
    {
        if (_lobbyGrain == null)
        {
            throw new NullReferenceException();
        }

        return _lobbyGrain.SetPlayerNumber(this, number);
    }

    public Task<int> GetScore()
    {
        return Task.FromResult(State.Score);
    }

    public async Task AddPlayerToQueue()
    {
        await GrainFactory.GetGrain<IMatchmakingGrain>(0).AddPlayerToQueue(this);
    }

    public Task<bool> HasLobby()
    {
        return Task.FromResult(_lobbyGrain != null);
    }

    public async Task AddScore()
    {
        State.Score++;
        await WriteStateAsync();
    }
}