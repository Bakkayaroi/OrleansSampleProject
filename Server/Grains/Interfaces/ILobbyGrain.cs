namespace Server.Grains.Interfaces;

public interface ILobbyGrain : IGrainWithGuidKey, IDisposable
{
    void AddPlayers(List<IPlayerGrain> players);
    Task<List<string>> GetPlayers();
    Task SetPlayerNumber(IPlayerGrain playerGrain, int number);
}