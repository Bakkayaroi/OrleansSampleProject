using Server.Grains.Interfaces;
using Server.Models;

namespace Server;

public interface IPlayerClient : IGrainObserver, IDisposable
{
    Task LobbyOpened(ILobbyGrain lobbyGrain);
    Task LobbyClosed(RoundResultModel roundResultModel);
}