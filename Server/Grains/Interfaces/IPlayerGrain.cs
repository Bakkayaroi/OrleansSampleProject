using Server.Models;

namespace Server.Grains.Interfaces;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task Subscribe(IPlayerClient observer);
    Task UnSubscribe(IPlayerClient observer);
    Task NotifyLobbyOpened(ILobbyGrain lobbyGrain);
    Task NotifyLobbyClosed(RoundResultModel roundResultModel);
    Task SendNumber(int number);
    Task AddScore();
    Task<int> GetScore();
    Task AddPlayerToQueue();
    Task<bool> HasLobby();
}