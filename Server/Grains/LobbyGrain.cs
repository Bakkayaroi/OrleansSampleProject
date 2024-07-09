using Server.Grains.Interfaces;
using Server.Models;

namespace Server.Grains;

public class LobbyGrain : Grain, ILobbyGrain
{
    private int _targetNumber;
    private List<IPlayerGrain> _players;
    private Dictionary<IPlayerGrain, int> _playerNumbers;
    private IDisposable _roundTimer;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _targetNumber = new Random().Next(0, 101);
        _playerNumbers = new();
        _roundTimer = RegisterTimer(EndRound, null, TimeSpan.FromMinutes(2), TimeSpan.FromMilliseconds(-1));
        return base.OnActivateAsync(cancellationToken);
    }

    private Task EndRound(object? _)
    {
        var winner = GetWinner();
        var winnerGrains = winner.Key;
        var winnerNumber = winner.Value;
        winnerGrains.AddScore();

        foreach (var playerGrain in _players)
        {
            playerGrain.NotifyLobbyClosed(new RoundResultModel
            {
                TargetNumber = _targetNumber,
                WinnerPlayer = winnerGrains.GetPrimaryKeyString(),
                WinnerNumber = winnerNumber,
            });
        }

        return Task.CompletedTask;
    }

    private KeyValuePair<IPlayerGrain, int> GetWinner()
    {
        return _playerNumbers.MinBy(p => Math.Abs(p.Value - _targetNumber));
    }

    private void CheckRoundCompletion()
    {
        if (_playerNumbers.Count == _players.Count)
        {
            _roundTimer?.Dispose();
            EndRound(null);
        }
    }

    public void AddPlayers(List<IPlayerGrain> players)
    {
        _players = players;

        foreach (var playerGrain in _players)
        {
            playerGrain.NotifyLobbyOpened(this);
        }
    }

    public Task<List<string>> GetPlayers()
    {
        return Task.FromResult(_players.Select(x => x.GetPrimaryKeyString()).ToList());
    }

    public Task SetPlayerNumber(IPlayerGrain playerGrain, int number)
    {
        _playerNumbers.Add(playerGrain, number);
        CheckRoundCompletion();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _roundTimer.Dispose();
        _playerNumbers.Clear();
        _players.Clear();
    }
}