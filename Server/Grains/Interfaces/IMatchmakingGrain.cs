namespace Server.Grains.Interfaces;

public interface IMatchmakingGrain : IGrainWithIntegerKey
{
    Task AddPlayerToQueue(IPlayerGrain player);
}